using CellexalVR.General;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
#if UNITY_EDITOR
using System.Reflection;
#endif

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Class for events that the keyboard trigger.
    /// </summary>
    [Serializable]
    public class KeyboardEvent : UnityEvent<string> { }

    /// <summary>
    /// Handles the keyboard and what happens when keys are pressed.
    /// </summary>
    public abstract class KeyboardHandler : MonoBehaviour
    {
        public CellexalVR.General.ReferenceManager referenceManager;
        public GameObject keysParentObject;
        public TMPro.TextMeshPro output;
        public List<TMPro.TextMeshPro> additionalOutputs;
        public float height = 5f;
        public float anglePerUnit = 5f;
        public float distance = 8f;
        public string placeholder = "Enter a name";
        public bool clearOnEnter = true;
        public KeyboardEvent OnEdit;
        public KeyboardEvent OnEditMultiuser;
        public KeyboardEvent OnEnter;
        public KeyboardEvent OnAnnotate;
        //public GameObject testCube;

        protected KeyboardPanel[] sortedKeys;
        [HideInInspector]
        public Vector2 minPos;
        [HideInInspector]
        public Vector2 maxPos;

        private bool displayingPlaceHolder = true;
        protected string prefabFolderPath = "Assets/Prefabs/Keyboards";
        protected string prefabPath;

        public int CurrentLayout { get; protected set; }
        abstract public string[][] Layouts { get; protected set; }

        public virtual void Shift() { }

        public virtual void NumChar() { }

        protected virtual void Start()
        {
            //Clear();
            SwitchLayout(Layouts[0]);
            //GatherKeys();
            CellexalEvents.GraphsUnloaded.AddListener(Clear);

        }
        public Vector4 ScaleCorrection()
        {
            float scaleCorrectionX = (maxPos.x - minPos.x) / (maxPos.y - minPos.y);
            return new Vector4(scaleCorrectionX, 1f, 1f, 1f);
        }

        /// <summary>
        /// Sets the materials on all keys. See <see cref="ClickablePanel.SetMaterials(Material, Material, Material)"/>.
        /// </summary>
        public void SetMaterials(Material keyNormalMaterial, Material keyHighlightMaterial, Material keyPressedMaterial)
        {
            if (sortedKeys == null || sortedKeys.Length == 0)
            {
                GatherKeys();
            }
            // set up the scale correction shader property so the rings on the keyboard are properly displayed
            Material adjustedKeyNormalMaterial = new Material(keyNormalMaterial);
            Material adjustedKeyHighlightedMaterial = new Material(keyHighlightMaterial);
            Material adjustedKeyPressedMaterial = new Material(keyPressedMaterial);
            Vector4 scaleCorrection = ScaleCorrection();
            adjustedKeyNormalMaterial.SetVector("_ScaleCorrection", scaleCorrection);
            adjustedKeyHighlightedMaterial.SetVector("_ScaleCorrection", scaleCorrection);
            adjustedKeyPressedMaterial.SetVector("_ScaleCorrection", scaleCorrection);

            //foreach (ClickablePanel panel in sortedKeys)
            //{
            //    panel.SetMaterials(adjustedKeyNormalMaterial, adjustedKeyHighlightedMaterial, adjustedKeyPressedMaterial);
            //}

            foreach (ClickablePanel panel in GetComponentsInChildren<ClickablePanel>(true))
            {
                panel.SetMaterials(adjustedKeyNormalMaterial, adjustedKeyHighlightedMaterial, adjustedKeyPressedMaterial, scaleCorrection);
            }
        }

        /// <summary>
        /// Helper class to sort keyboarditems based on their position on the keyboard.
        /// </summary>
        private class KeyboardItemComparer : IComparer<KeyboardItem>
        {
            public int Compare(KeyboardItem a, KeyboardItem b)
            {
                return a.position.y == b.position.y ? (int)(a.position.x - b.position.x) : (int)(b.position.y - a.position.y);
            }
        }

        protected void GatherKeys()
        {
            GatherKeys(this);
        }

        /// <summary>
        /// Gathers the keys from the scene.
        /// </summary>
        protected void GatherKeys(KeyboardHandler prefabInstance)
        {
            prefabInstance.minPos = new Vector2(float.MaxValue, float.MaxValue);
            prefabInstance.maxPos = new Vector2(float.MinValue, float.MinValue);

            KeyboardItem[] items = prefabInstance.GetComponentsInChildren<KeyboardItem>(true);
            KeyboardItem[] itemsUnderKeyObject = prefabInstance.keysParentObject.GetComponentsInChildren<KeyboardItem>(true);
            prefabInstance.sortedKeys = prefabInstance.GetComponentsInChildren<KeyboardPanel>(true);
            // sort by position, y first then x
            Array.Sort(itemsUnderKeyObject, prefabInstance.sortedKeys, new KeyboardItemComparer());
            // find the smallest and largest positions for later
            foreach (var item in items)
            {
                if (!item.hasKeyboardMaterial)
                {
                    continue;
                }
                Vector2 thisMinPos = item.position - new Vector2(item.size.x / 2f, 0f);
                Vector2 thisMaxPos = item.position + new Vector2(item.size.x / 2f, item.size.y);
                if (thisMinPos.x < minPos.x)
                {
                    prefabInstance.minPos.x = thisMinPos.x;
                }
                else if (thisMaxPos.x > maxPos.x)
                {
                    prefabInstance.maxPos.x = thisMaxPos.x;
                }
                if (thisMinPos.y < minPos.y)
                {
                    prefabInstance.minPos.y = thisMinPos.y;
                }
                else if (thisMaxPos.y > maxPos.y)
                {
                    prefabInstance.maxPos.y = thisMaxPos.y;
                }
            }
        }

        /// <summary>
        /// Switches the layout of the keyboard.
        /// </summary>
        /// <param name="layout">The new layout.</param>
        protected void SwitchLayout(string[] layout)
        {
            if (sortedKeys == null || layout.Length != sortedKeys.Length)
            {
                // none, too many or too few keys found
                GatherKeys();

                if (layout.Length != sortedKeys.Length)
                {
                    Debug.LogError("Invalid number of keys on " + gameObject.name + ", string array layout length: " + layout.Length + ", number of found Keyboard panel gameobjects: " + sortedKeys.Length);
                    return;
                }
            }
            // everything seems ok, switch the layout
            for (int i = 0; i < sortedKeys.Length; ++i)
            {
                sortedKeys[i].GetComponentInChildren<KeyboardPanel>().Text = layout[i];
            }
        }

        /// <summary>
        /// Adds a character to the output.
        /// </summary>
        /// <param name="s"></param>
        public void AddText(string s, bool invokeMultiuserEvent)
        {
            if (displayingPlaceHolder)
            {
                output.text = s.ToString();
                displayingPlaceHolder = false;
            }
            else
            {
                output.text += s;
            }

            if (OnEdit != null)
            {
                OnEdit.Invoke(Text());
            }

            if (invokeMultiuserEvent)
            {
                OnEditMultiuser.Invoke(s.ToString());
            }
        }


        /// <summary>
        /// Removes the last typed letter.
        /// </summary>
        public void BackSpace()
        {
            if (output.text.Length == 1)
            {
                Clear();
            }
            else if (!displayingPlaceHolder)
            {
                output.text = output.text.Remove(output.text.Length - 1);
            }
            if (OnEdit != null)
            {
                OnEdit.Invoke(Text());
            }
        }

        /// <summary>
        /// Clears the output.
        /// </summary>
        public void Clear()
        {
            output.text = placeholder;
            displayingPlaceHolder = true;
            if (OnEdit != null)
            {
                OnEdit.Invoke(Text());
            }
        }

        /// <summary>
        /// Colors all graphs based on what was typed.
        /// </summary>
        public void SubmitOutput(bool invoke = true)
        {
            foreach (var o in additionalOutputs)
            {
                o.text = output.text;
            }
            if (invoke && OnEnter != null)
            {
                OnEnter.Invoke(Text());
            }
            //referenceManager.cellManager.ColorGraphsByGene(output.text);
            //referenceManager.previousSearchesList.AddEntry(output.text, Extensions.Definitions.Measurement.GENE, referenceManager.graphManager.GeneExpressionColoringMethod);
            //referenceManager.gameManager.InformColorGraphsByGene(output.text);
            if (clearOnEnter)
            {
                Clear();
            }
        }

        /// <summary>
        /// Returns the currently typed text.
        /// </summary>
        public string Text()
        {
            if (displayingPlaceHolder)
            {
                return "";
            }
            else
            {
                return output.text;
            }
        }

        public void DismissKeyboard()
        {
            additionalOutputs.Clear();
            gameObject.SetActive(false);
        }

        public void SetAllOutputs(string text)
        {
            output.text = text;
            foreach (var o in additionalOutputs)
            {
                o.text = text;
            }
        }

        /// <summary>
        /// Converts a direction to a uv2 coordinate on the keyboard.
        /// This function can return uv2 coordinates that are not actually on the keyboard if there is no keyboard in that direction.
        /// </summary>
        /// <param name="dir">A direction starting from the keyboard's local (0, 0, 0) position, preferably pointing towards the keyboard.</param>
        /// <returns>A uv2 coordinate where the <paramref name="dir"/> points to.</returns>
        public Vector2 ToUv2Coord(Vector3 dir)
        {
            float angleMultiplier = anglePerUnit == 0f ? 1f : anglePerUnit;
            Vector3 dirLocalSpace = transform.InverseTransformPoint(dir);
            //a.y = 0f;

            // y coordinate
            //float heightInWorldSpace = Mathf.Cos(Vector3.Angle(Vector3.up, transform.up) * Mathf.Deg2Rad);
            float keyboardYCoord = dirLocalSpace.y / height;

            // x coordinate
            dirLocalSpace.y = 0f;
            Vector3 a = Vector3.right;
            Vector3 b = dirLocalSpace + new Vector3(distance, 0f, 0f);
            //b.y = 0f;
            float angle = Vector3.SignedAngle(a, b, Vector3.up);
            float keyboardSizeX = (maxPos.x - minPos.x) * angleMultiplier;
            float keyboardAngleSizeNegative = minPos.x * angleMultiplier;
            float angleFromZero = Mathf.Abs(keyboardAngleSizeNegative) + angle;

            return new Vector2(angleFromZero / keyboardSizeX, keyboardYCoord);
            //print("angleMultiplier " + angleMultiplier + " a " + V2S(a) + " b " + V2S(b) + " angle " + angle + " keyboardSizeX " + keyboardSizeX +
            //    " keyboardAngleSizeNegative " + keyboardAngleSizeNegative + " angleFromZero " + angleFromZero +
            //    " keyboardYCoord " + keyboardYCoord + " laserCoords " + V2S(laserCoords));
        }

        private string V2S(Vector3 v)
        {
            return "(" + v.x + ", " + v.y + ", " + v.z + ")";
        }

        private string V2S(Vector2 v)
        {
            return "(" + v.x + ", " + v.y + ")";
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (referenceManager == null && gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<General.ReferenceManager>();
            }

            if (!System.IO.Directory.Exists(prefabFolderPath))
            {
                System.IO.Directory.CreateDirectory(prefabFolderPath);
            }
        }

        /// <summary>
        /// Helper method for inherited classes to open the correct prefab. Remember to call <see cref="ClosePrefab(GameObject)"/> after.
        /// </summary>
        /// <param name="prefab">The opened prefab.</param>
        /// <param name="scriptOnPrefab">This script, but on the prefab.</param>
        protected void OpenPrefab<T>(out GameObject outerMostPrefab, out T scriptOnPrefab) where T : KeyboardHandler
        {
            UnityEditor.EditorUtility.DisplayProgressBar("Building keyboard", "Getting correct prefab", 0f);
            GameObject outerMostPrefabInstance = UnityEditor.PrefabUtility.GetOutermostPrefabInstanceRoot(gameObject);
            prefabPath = UnityEditor.PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(outerMostPrefabInstance);

            UnityEditor.EditorUtility.DisplayProgressBar("Building keyboard", "Applying overrides", 0.1f);
            UnityEditor.Undo.RecordObject(outerMostPrefabInstance, "Apply keyboard override");
            UnityEditor.PrefabUtility.ApplyPrefabInstance(outerMostPrefabInstance, UnityEditor.InteractionMode.AutomatedAction);
            UnityEditor.PrefabUtility.SaveAsPrefabAssetAndConnect(outerMostPrefabInstance, prefabPath, UnityEditor.InteractionMode.AutomatedAction);
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(outerMostPrefabInstance.scene);
            UnityEditor.EditorUtility.DisplayProgressBar("Building keyboard", "Opening prefab", 0.2f);
            outerMostPrefab = UnityEditor.PrefabUtility.LoadPrefabContents(prefabPath);
            scriptOnPrefab = outerMostPrefab.GetComponentInChildren<T>(true);
            if (!scriptOnPrefab)
            {
                Debug.LogError("No appropriate script (" + typeof(T).ToString() + ") found in prefab at " + prefabPath);
                UnityEditor.PrefabUtility.UnloadPrefabContents(outerMostPrefab);
                UnityEditor.EditorUtility.ClearProgressBar();
                return;
            }

            UnityEditor.EditorUtility.DisplayProgressBar("Building keyboard", "Building keyboard", 0.3f);
            UnityEditor.AssetDatabase.StartAssetEditing();
        }

        /// <summary>
        /// Helper method for inherited clases to close the correct prefab.
        /// </summary>
        /// <param name="prefab">The <see cref="GameObject"/> that <see cref="OpenPrefab{T}(out GameObject, out T)"/> returns</param>
        protected void ClosePrefab(GameObject prefab)
        {
            UnityEditor.AssetDatabase.StopAssetEditing();
            UnityEditor.EditorUtility.DisplayProgressBar("Building keyboard", "Saving prefab", 0.9f);
            UnityEditor.PrefabUtility.SaveAsPrefabAssetAndConnect(prefab, prefabPath, UnityEditor.InteractionMode.AutomatedAction);
            UnityEditor.PrefabUtility.UnloadPrefabContents(prefab);


            UnityEditor.EditorUtility.ClearProgressBar();
        }


        /// <summary>
        /// Builds the keyboard by creating the meshes and positioning everything.
        /// </summary>
        public virtual void BuildKeyboard(KeyboardHandler prefab)
        {

            string keyboardMeshesBaseFolder = prefabFolderPath + "/KeyboardMeshes";
            if (!UnityEditor.AssetDatabase.IsValidFolder(keyboardMeshesBaseFolder))
            {
                UnityEditor.AssetDatabase.CreateFolder(prefabFolderPath, "KeyboardMeshes");
            }

            string keyboardMeshesFolder = keyboardMeshesBaseFolder + "/" + prefab.name;
            if (!UnityEditor.AssetDatabase.IsValidFolder(keyboardMeshesFolder))
            {
                UnityEditor.AssetDatabase.CreateFolder(keyboardMeshesBaseFolder, prefab.name);
            }
            else
            {
                foreach (var asset in UnityEditor.AssetDatabase.FindAssets("", new string[] { keyboardMeshesFolder }))
                {
                    UnityEditor.AssetDatabase.DeleteAsset(UnityEditor.AssetDatabase.GUIDToAssetPath(asset));
                }
            }

            prefab.GatherKeys(prefab);
            Vector2 size = prefab.maxPos - prefab.minPos;

            float radiansPerUnit = anglePerUnit / 180f * Mathf.PI;

            foreach (KeyboardItem item in prefab.GetComponentsInChildren<KeyboardItem>())
            {
                Vector3 position;
                float keyAngleDegrees = 0;
                if (anglePerUnit != 0f)
                {
                    keyAngleDegrees = anglePerUnit * -item.position.x;
                    float keyAngleRadians = radiansPerUnit * -item.position.x;
                    position = new Vector3(Mathf.Cos(keyAngleRadians) - 1f, 0, Mathf.Sin(keyAngleRadians)) * distance;

                    item.transform.localRotation = Quaternion.Euler(0f, 90 - keyAngleDegrees, 0f);
                }
                else
                {
                    position = new Vector3(item.position.x / height, item.position.y / height, 0);
                }
                float yPos = item.position.y * height / size.y;
                position += new Vector3(0f, yPos, 0f);
                item.transform.localPosition = position;
                // scale so everything fits
                float scale = height / size.y;
                item.transform.localScale = new Vector3(1f, 1f, 1f);
                ClickablePanel panel = item.GetComponentInChildren<ClickablePanel>();
                if (panel != null)
                {
                    if (panel is KeyboardPanel keyboardPanel)
                    {
                        UnityEditor.Undo.RecordObject(keyboardPanel, "Keyboardpanel build keyboard");
                        keyboardPanel.referenceManager = prefab.referenceManager;
                        keyboardPanel.handler = prefab;

                        TextMeshPro text = item.GetComponentInChildren<TextMeshPro>();
                        text.margin = new Vector4(0.25f, 0f, 0.25f, 0.5f);
                        if (keyboardPanel.keyType == KeyboardPanel.Type.Enter)
                        {
                            text.transform.localPosition = new Vector3(0f, 0f, -0.1f * anglePerUnit / 3f);
                        }
                        else
                        {
                            text.transform.localPosition = new Vector3(0f, 0f, -0.01f);
                        }
                        text.transform.localScale = Vector3.one * (height / ((size.y - 1) / 4));

                    }
                    Vector2 min = item.position - new Vector2(item.size.x / 2f, 0f) - minPos;
                    Vector2 max = item.position + new Vector2(item.size.x / 2f, item.size.y) - minPos;
                    Vector2 smallUV = new Vector2(min.x / size.x, min.y / size.y);
                    Vector2 largeUV = new Vector2(max.x / size.x, max.y / size.y);
                    panel.CenterUV = (smallUV + largeUV) / 2f;
                    //print("panel " + item.name + " smalluv (" + smallUV.x + ", " + smallUV.y + ") largeuv (" + largeUV.x + ", " + largeUV.y + ") keyangle " + keyAngleDegrees);
                    Mesh mesh = CreateNineSlicedQuad(smallUV, largeUV, item.size, radiansPerUnit, size);
                    string assetName = keyboardMeshesFolder + "/KeyboardMesh_" + item.name + ".mesh";
                    assetName = UnityEditor.AssetDatabase.GenerateUniqueAssetPath(assetName);
                    UnityEditor.AssetDatabase.CreateAsset(mesh, assetName);
                    panel.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                    panel.GetComponent<MeshFilter>().mesh = mesh;
                    if (panel.GetComponent<MeshCollider>())
                    {
                        DestroyImmediate(panel.GetComponent<MeshCollider>());
                    }
                    if (panel.GetComponent<BoxCollider>() == null)
                    {
                        panel.gameObject.AddComponent<BoxCollider>();
                    }
                    var boxCollider = panel.gameObject.GetComponent<BoxCollider>();
                    boxCollider.center = mesh.bounds.center;
                    boxCollider.size = mesh.bounds.size;
                    panel.transform.localScale = new Vector3(1f, 1f, 1f);

                }
                //print("assigning (" + smallUV.x + ", " + smallUV.y + ") (" + largeUV.x + ", " + largeUV.y + ") to " + panel.Text);
            }


            prefab.SwitchLayout(Layouts[0]);
            if (prefab.output)
            {
                prefab.output.text = placeholder;
            }
        }

        /// <summary>
        /// Creates a nine sliced quad that is used for the panels on the keyboard. This lets us use a texture with a border that won't be stretched over non-square panels. 
        /// Meshes will be curved along a circle arc with the angle <paramref name="anglePerUnit"/> multiplied by the x component of <paramref name="size"/> and with the radius <paramref name="distance"/>.
        /// </summary>
        /// <param name="uv2min">The smallest uv2 coordinates. The uv2 should reflect the panel's position on the keyboard.</param>
        /// <param name="uv2max">The largest uv2 coordinates.</param>
        /// <param name="size">This panel's size.</param>
        /// <param name="anglePerUnit">Radians per unit of <paramref name="size"/>. Affects how wide the panel becomes.</param>
        /// <param name="keyboardSize">The size of the keyboard, in the same units as <paramref name="size"/>.</param>
        /// <returns></returns>
        public Mesh CreateNineSlicedQuad(Vector2 uv2min, Vector2 uv2max, Vector2 size, float anglePerUnit, Vector2 keyboardSize)
        {
            // left, right, bottom and top margins
            float l = 0.1f, r = 0.1f;
            float b = 0.1f, t = 0.1f;
            float uvl = l / 2f, uvr = r / 2f;
            float uvb = b / 2f, uvt = t / 2f;
            int xSegments = (int)size.x;
            float maxXCoord = size.x / 2f;
            float minXCoord = -maxXCoord;
            float adjHeight = height / keyboardSize.y;
            float maxYCoord = size.y * adjHeight;

            // vertices need their coordinates scaled, uv does not 
            //float adjl = l * adjSize.x;
            //float adjr = r * adjSize.x;
            float adjb = b * adjHeight;
            float adjt = t * adjHeight;
            anglePerUnit = -anglePerUnit;
            // pre-calculate some values
            float[] cosVals = new float[] {
                Mathf.Cos(anglePerUnit * minXCoord) * distance - distance,
                Mathf.Cos(anglePerUnit * (minXCoord + l)) * distance - distance,
                Mathf.Cos(anglePerUnit * (maxXCoord - r)) * distance - distance,
                Mathf.Cos(anglePerUnit * maxXCoord) * distance - distance
            };
            float[] sinVals = new float[] {
                Mathf.Sin(anglePerUnit * minXCoord) * distance,
                Mathf.Sin(anglePerUnit * (minXCoord + l)) * distance,
                Mathf.Sin(anglePerUnit * (maxXCoord - r)) * distance,
                Mathf.Sin(anglePerUnit * maxXCoord) * distance
            };

            Vector3[] verts = new Vector3[16 + 4 * (xSegments - 1)];
            // set up the corners
            Vector3[] vertCorners = new Vector3[] {
                new Vector3(cosVals[0], maxYCoord, sinVals[0]), new Vector3(cosVals[0], maxYCoord - adjt, sinVals[0]), new Vector3(cosVals[0], adjb, sinVals[0]), new Vector3(cosVals[0], 0f, sinVals[0]),
                new Vector3(cosVals[1], maxYCoord, sinVals[1]), new Vector3(cosVals[1], maxYCoord - adjt, sinVals[1]), new Vector3(cosVals[1], adjb, sinVals[1]), new Vector3(cosVals[1], 0f, sinVals[1]),
                new Vector3(cosVals[2], maxYCoord, sinVals[2]), new Vector3(cosVals[2], maxYCoord - adjt, sinVals[2]), new Vector3(cosVals[2], adjb, sinVals[2]), new Vector3(cosVals[2], 0f, sinVals[2]),
                new Vector3(cosVals[3], maxYCoord, sinVals[3]), new Vector3(cosVals[3], maxYCoord - adjt, sinVals[3]), new Vector3(cosVals[3], adjb, sinVals[3]), new Vector3(cosVals[3], 0f, sinVals[3])
            };

            Array.Copy(vertCorners, 0, verts, 0, 8);
            Array.Copy(vertCorners, 8, verts, verts.Length - 8, 8);

            // do the same for the uv
            Vector2[] uv = new Vector2[verts.Length];
            Vector2[] uvCorners = new Vector2[] {
                new Vector2(0f, 1f),       new Vector2(0f, 1f - uvt),       new Vector2(0f, uvb),       new Vector2(0f, 0f),
                new Vector2(uvl, 1f),      new Vector2(uvl, 1f - uvt),      new Vector2(uvl, uvb),      new Vector2(uvl, 0f),
                new Vector2(1f - uvr, 1f), new Vector2(1f - uvr, 1f - uvt), new Vector2(1f - uvr, uvb), new Vector2(1f - uvr, 0f),
                new Vector2(1f, 1f),       new Vector2(1f, 1f - uvt),       new Vector2(1f, uvb),       new Vector2(1f, 0f)
            };

            Array.Copy(uvCorners, 0, uv, 0, 8);
            Array.Copy(uvCorners, 8, uv, uv.Length - 8, 8);

            // set up the middle segments
            float xCoordDiff = (maxXCoord - minXCoord) / xSegments;
            for (int i = 1, index = 8; i < xSegments; ++i)
            {
                float xposUV = i / size.x;
                float angle = anglePerUnit * (minXCoord + xCoordDiff * i);
                float xpos = Mathf.Cos(angle) * distance - distance;
                float zpos = Mathf.Sin(angle) * distance;

                verts[index] = new Vector3(xpos, maxYCoord, zpos);
                uv[index] = new Vector2(xposUV, 1f);
                index++;

                verts[index] = new Vector3(xpos, maxYCoord - adjt, zpos);
                uv[index] = new Vector2(xposUV, 1f - uvt);
                index++;

                verts[index] = new Vector3(xpos, adjb, zpos);
                uv[index] = new Vector2(xposUV, uvb);
                index++;

                verts[index] = new Vector3(xpos, 0f, zpos);
                uv[index] = new Vector2(xposUV, 0f);
                index++;
            }

            // fill in all the triangles, these fortunately look the same over the entire mesh
            int[] tris = new int[54 + 18 * (xSegments - 1)];
            for (int i = 0, vert = 0; i < tris.Length; ++vert)
            {
                tris[i++] = vert;
                tris[i++] = vert + 4;
                tris[i++] = vert + 5;
                tris[i++] = vert;
                tris[i++] = vert + 5;
                tris[i++] = vert + 1;
                // skip every 4th vertex as that is jumping to a new coloumn
                if (vert % 4 == 2)
                {
                    vert++;
                }
            }

            // uv2 should be the each vertex position relative to its position on the keyboard
            Vector2[] uv2 = new Vector2[verts.Length];

            float uv2l = l / keyboardSize.x, uv2r = r / keyboardSize.x;
            float uv2b = b / keyboardSize.y, uv2t = t / keyboardSize.y;
            // set up the corners 
            Vector2[] uv2Corners = new Vector2[] {
                new Vector2(uv2min.x,        uv2max.y), new Vector2(uv2min.x,        uv2max.y - uv2t), new Vector2(uv2min.x,        uv2min.y + uv2b), new Vector2(uv2min.x,        uv2min.y),
                new Vector2(uv2min.x + uv2l, uv2max.y), new Vector2(uv2min.x + uv2l, uv2max.y - uv2t), new Vector2(uv2min.x + uv2l, uv2min.y + uv2b), new Vector2(uv2min.x + uv2l, uv2min.y),
                new Vector2(uv2max.x - uv2r, uv2max.y), new Vector2(uv2max.x - uv2r, uv2max.y - uv2t), new Vector2(uv2max.x - uv2r, uv2min.y + uv2b), new Vector2(uv2max.x - uv2r, uv2min.y),
                new Vector2(uv2max.x,        uv2max.y), new Vector2(uv2max.x,        uv2max.y - uv2t), new Vector2(uv2max.x,        uv2min.y + uv2b), new Vector2(uv2max.x,        uv2min.y)
            };
            Array.Copy(uv2Corners, 0, uv2, 0, 8);
            Array.Copy(uv2Corners, 8, uv2, uv2.Length - 8, 8);

            // set up the middle segments
            float uv2XDiff = (uv2max.x - uv2min.x) / xSegments;
            for (int i = 1, index = 8; i < xSegments; ++i)
            {
                float xPos = uv2min.x + uv2XDiff * i;
                uv2[index] = new Vector2(xPos, uv2max.y);
                index++;
                uv2[index] = new Vector2(xPos, uv2max.y - uv2t);
                index++;
                uv2[index] = new Vector2(xPos, uv2min.y + uv2b);
                index++;
                uv2[index] = new Vector2(xPos, uv2min.y);
                index++;
            }

            Mesh mesh = new Mesh()
            {
                vertices = verts,
                triangles = tris,
                uv = uv,
                uv2 = uv2
            };
            mesh.RecalculateBounds();

            return mesh;
        }
#endif
    }

}
