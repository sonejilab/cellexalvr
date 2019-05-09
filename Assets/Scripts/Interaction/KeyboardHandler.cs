using UnityEngine;
using System.Collections.Generic;
using System;
using CellexalVR.General;
using UnityEngine.Events;
using TMPro;

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
    public class KeyboardHandler : MonoBehaviour
    {
        public CellexalVR.General.ReferenceManager referenceManager;
        public GameObject keysParentObject;
        public Material keyMaterial;
        public Mesh quadPrefab;
        public AutoCompleteList autoCompleteList;
        public TMPro.TextMeshPro output;
        public float height = 5f;
        public float anglePerUnit = 5f;
        public float distance = 8f;
        public enum Layout { Uppercase, Lowercase, Special }
        public Layout currentLayout = Layout.Lowercase;
        public string placeholder = "Enter a gene name";
        public KeyboardEvent OnEdit;
        public KeyboardEvent OnEnter;

        private bool keyLayoutUppercase = false;
        private KeyboardPanel[] sortedKeys;
        private Vector2 minPos;
        private Vector2 maxPos;

        // these are the layouts that the keyboard can switch between
        private string[] uppercase = { "Q", "W", "E", "R", "T", "Y", "U", "I", "O", "P",
                                       "Shift", "A", "S", "D", "F", "G", "H", "J", "K", "L",
                                       "123\n!#%", "Z", "X", "C", "V", "B", "N", "M", "Back", "Clear",
                                       "Enter"};

        private string[] lowercase = { "q", "w", "e", "r", "t", "y", "u", "i", "o", "p",
                                       "Shift", "a", "s", "d", "f", "g", "h", "j", "k", "l",
                                       "123\n!#%", "z", "x", "c", "v", "b", "n", "m", "Back", "Clear",
                                       "Enter"};

        private string[] special = {   "1", "2", "3", "4", "5", "6", "7", "8", "9", "0",
                                       "Shift", "!", "#", "%", "&", "/", "(", ")", "=", "@",
                                       "ABC\nabc", "\\", "-", "_", ".", ":", ",", ";", "Back", "Clear",
                                       "Enter"};

        private bool displayingPlaceHolder = true;

        void Start()
        {
            //Clear();
            SwitchLayout(Layout.Lowercase);
            GatherKeys();
            CellexalEvents.GraphsUnloaded.AddListener(Clear);
        }

        /// <summary>
        /// Sets the materials on all keys. See <see cref="ClickablePanel.SetMaterials(Material, Material, Material)"/>.
        /// </summary>
        public void SetMaterials(Material keyNormalMaterial, Material keyHighlightMaterial, Material keyPressedMaterial)
        {
            if (sortedKeys == null)
            {
                GatherKeys();
            }
            foreach (ClickablePanel panel in sortedKeys)
            {
                panel.SetMaterials(keyNormalMaterial, keyHighlightMaterial, keyPressedMaterial);
            }
        }

        /// <summary>
        /// Gathers the keys from the scene.
        /// </summary>
        private void GatherKeys()
        {
            minPos = new Vector2(float.MaxValue, float.MaxValue);
            maxPos = new Vector2(float.MinValue, float.MinValue);

            KeyboardItem[] items = GetComponentsInChildren<KeyboardItem>();
            sortedKeys = GetComponentsInChildren<KeyboardPanel>();
            // sort by position, y first then x
            //Array.Sort(items, (KeyboardItem a, KeyboardItem b) => a.position.y == b.position.y ? (int)(a.position.x - b.position.x) : (int)(a.position.y - b.position.y));
            // find the smallest and largest positions for later
            foreach (var item in items)
            {
                Vector2 thisMinPos = item.position - item.size / 2;
                Vector2 thisMaxPos = item.position + item.size / 2;
                if (thisMinPos.x < minPos.x)
                {
                    minPos.x = thisMinPos.x;
                }
                else if (thisMaxPos.x > maxPos.x)
                {
                    maxPos.x = thisMaxPos.x;
                }
                if (thisMinPos.y < minPos.y)
                {
                    minPos.y = thisMinPos.y;
                }
                else if (thisMaxPos.y > maxPos.y)
                {
                    maxPos.y = thisMaxPos.y;
                }
            }
        }

        /// <summary>
        /// Adds a character to the output.
        /// </summary>
        /// <param name="c"></param>
        public void AddCharacter(char c)
        {
            if (displayingPlaceHolder)
            {
                output.text = c + "";
                displayingPlaceHolder = false;
            }
            else
            {
                output.text += c;
            }
            if (OnEdit != null)
            {
                OnEdit.Invoke(Text());
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
        /// Switches between uppercase and lowercase layout.
        /// </summary>
        public void Shift()
        {
            if (currentLayout == Layout.Uppercase)
            {
                SwitchLayout(Layout.Lowercase);
            }
            else if (currentLayout == Layout.Lowercase)
            {
                SwitchLayout(Layout.Uppercase);
            }
        }

        /// <summary>
        /// Switches the layout of the keyboard.
        /// </summary>
        /// <param name="layout">The new layout.</param>
        public void SwitchLayout(Layout layout)
        {
            string[] layoutToSwitchTo;
            if (layout == Layout.Lowercase)
            {
                layoutToSwitchTo = lowercase;
            }
            else if (layout == Layout.Uppercase)
            {
                layoutToSwitchTo = uppercase;
            }
            else if (layout == Layout.Special)
            {
                layoutToSwitchTo = special;
            }
            else
            {
                Debug.LogError("Invalid layout selected in KeyboardHandler.cs");
                return;
            }

            if (sortedKeys == null || layoutToSwitchTo.Length != sortedKeys.Length)
            {
                // none, too many or too few keys found
                GatherKeys();

                if (layoutToSwitchTo.Length != sortedKeys.Length)
                {
                    Debug.LogError("Invalid number of keys in KeyboardHandler.cs, string array layout length: " + layoutToSwitchTo.Length + ", number of found Keyboard panel gameobjects: " + sortedKeys.Length);
                    return;
                }
            }
            // everything seems ok, switch the layout
            for (int i = 0; i < sortedKeys.Length; ++i)
            {
                sortedKeys[i].GetComponentInChildren<KeyboardPanel>().Text = layoutToSwitchTo[i];
            }
            currentLayout = layout;
        }

        /// <summary>
        /// Colors all graphs based on what was typed.
        /// </summary>
        public void SubmitOutput(bool invoke = true)
        {
            if (invoke && OnEnter != null)
            {
                OnEnter.Invoke(Text());
            }
            //referenceManager.cellManager.ColorGraphsByGene(output.text);
            //referenceManager.previousSearchesList.AddEntry(output.text, Extensions.Definitions.Measurement.GENE, referenceManager.graphManager.GeneExpressionColoringMethod);
            //referenceManager.gameManager.InformColorGraphsByGene(output.text);
            Clear();
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (referenceManager == null && gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<General.ReferenceManager>();
            }
            //BuildKeyboard();
        }

        /// <summary>
        /// Builds the keyboard by creating the meshes and positioning everything.
        /// </summary>
        public void BuildKeyboard()
        {
            GatherKeys();
            referenceManager.previousSearchesList.BuildPreviousSearchesList();

            Vector2 size = maxPos - minPos;
            SwitchLayout(currentLayout);
            output.text = placeholder;
            float radiansPerUnit = anglePerUnit / 180f * Mathf.PI;

            KeyboardPanel[] keys = keysParentObject.GetComponentsInChildren<KeyboardPanel>();
            foreach (KeyboardItem item in GetComponentsInChildren<KeyboardItem>())
            {
                Vector3 position;
                if (anglePerUnit != 0f)
                {
                    float keyAngleDegrees = anglePerUnit * -item.position.x;
                    float keyAngleRadians = radiansPerUnit * -item.position.x;
                    position = new Vector3(Mathf.Cos(keyAngleRadians), 0, Mathf.Sin(keyAngleRadians)) * distance;
                    item.transform.localRotation = Quaternion.Euler(0f, 90 - keyAngleDegrees, 0f);
                }
                else
                {
                    position = new Vector3(item.position.x, 0, item.position.y);
                }
                float yPos = (size.y - item.position.y + minPos.y) * height / size.y;
                position += new Vector3(0f, yPos, 0f);
                item.transform.localPosition = position;
                // scale so everything fits
                float scale = height / size.y;
                item.transform.localScale = new Vector3(1f, 1f, 1f);
                ClickablePanel panel = item.GetComponentInChildren<ClickablePanel>();
                if (panel != null)
                {
                    if (panel is KeyboardPanel)
                    {
                        KeyboardPanel keyboardPanel = (KeyboardPanel)panel;
                        keyboardPanel.referenceManager = referenceManager;
                        panel.GetComponent<MeshRenderer>().sharedMaterial = keyMaterial;
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
                        text.transform.localScale = Vector3.one * (height / 3f);

                    }

                    Vector2 min = item.position - item.size / 2f - minPos;
                    Vector2 max = item.position + item.size / 2f - minPos;
                    Vector2 smallUV = new Vector2(min.x / size.x, max.y / size.y);
                    Vector2 largeUV = new Vector2(max.x / size.x, min.y / size.y);
                    panel.CenterUV = (smallUV + largeUV) / 2f;
                    Mesh mesh = CreateNineSlicedQuad(smallUV, largeUV, item.size, radiansPerUnit, distance);
                    panel.transform.localRotation = Quaternion.Euler(0f, -90f, 0f);
                    panel.GetComponent<MeshFilter>().sharedMesh = mesh;
                    panel.GetComponent<MeshCollider>().sharedMesh = mesh;
                    panel.transform.localScale = new Vector3(1f, scale, 1f);

                }

                //print("assigning (" + smallUV.x + ", " + smallUV.y + ") (" + largeUV.x + ", " + largeUV.y + ") to " + panel.Text);

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
        /// <param name="distance">The radius of the circle's arc. Affects how curved the panel becomes.</param>
        /// <returns></returns>
        private Mesh CreateNineSlicedQuad(Vector2 uv2min, Vector2 uv2max, Vector2 size, float anglePerUnit, float distance)
        {
            // left, right, bottom and top margins
            float l = 0.1f, r = 0.1f;
            float b = 0.1f, t = 0.1f;
            int xSegments = (int)size.x;
            float maxXCoord = xSegments / 2f;
            float minXCoord = -maxXCoord;
            float maxYCoord = size.y;
            // vertices need their coordinates scaled, uv does not
            float adjl = l * size.y;
            float adjr = r * size.y;
            float adjb = b * size.y;
            float adjt = t * size.y;
            anglePerUnit = -anglePerUnit;
            // pre-calculate some values
            float[] cosVals = new float[] {
                Mathf.Cos(anglePerUnit * minXCoord) * distance - distance,
                Mathf.Cos(anglePerUnit * (minXCoord + adjl)) * distance - distance,
                Mathf.Cos(anglePerUnit * (maxXCoord - adjr)) * distance - distance,
                Mathf.Cos(anglePerUnit * maxXCoord) * distance - distance
            };
            float[] sinVals = new float[] {
                Mathf.Sin(anglePerUnit * minXCoord) * distance,
                Mathf.Sin(anglePerUnit * (minXCoord + adjl)) * distance,
                Mathf.Sin(anglePerUnit * (maxXCoord - adjr)) * distance,
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
            Vector2[] uv = new Vector2[16 + 4 * (xSegments - 1)];
            Vector2[] uvCorners = new Vector2[] {
                new Vector2(0f, 1f),     new Vector2(0f, 1f - t),     new Vector2(0f, b),     new Vector2(0f, 0f),
                new Vector2(l, 1f),      new Vector2(l, 1f - t),      new Vector2(l, b),      new Vector2(l, 0f),
                new Vector2(1f - r, 1f), new Vector2(1f - r, 1f - t), new Vector2(1f - r, b), new Vector2(1f - r, 0f),
                new Vector2(1f, 1f),     new Vector2(1f, 1f - t),     new Vector2(1f, b),     new Vector2(1f, 0f)
            };

            Array.Copy(uvCorners, 0, uv, 0, 8);
            Array.Copy(uvCorners, 8, uv, uv.Length - 8, 8);

            // set up the middle segments
            for (int i = 0; i < xSegments - 1; ++i)
            {
                int index = 8 + i * 4;
                float xposUV = (i + 1f) / xSegments;
                float angle = anglePerUnit * (minXCoord + (maxXCoord - minXCoord) * xposUV);
                float xpos = Mathf.Cos(angle) * distance - distance;
                float zpos = Mathf.Sin(angle) * distance;

                verts[index] = new Vector3(xpos, maxYCoord, zpos);
                uv[index] = new Vector3(xposUV, 1f, 0f);
                index++;

                verts[index] = new Vector3(xpos, maxYCoord - adjt, zpos);
                uv[index] = new Vector3(xposUV, 1f - t, 0f);
                index++;

                verts[index] = new Vector3(xpos, adjb, zpos);
                uv[index] = new Vector3(xposUV, b, 0f);
                index++;

                verts[index] = new Vector3(xpos, 0f, zpos);
                uv[index] = new Vector3(xposUV, 0f, 0f);
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

            // uv2 based on uv but should be in real world coordinates
            Vector2[] uv2 = new Vector2[uv.Length];
            Vector2 uv2Diff = uv2max - uv2min;
            for (int i = 0; i < uv2.Length; ++i)
            {
                uv2[i] = Vector2.Scale(uv[i], uv2Diff) + uv2min;
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
#if UNITY_EDITOR
    /// <summary>
    /// Editor class for the <see cref="KeyboardHandler"/> to add a "Build keyboard" button.
    /// </summary>
    [UnityEditor.CustomEditor(typeof(KeyboardHandler))]
    [UnityEditor.CanEditMultipleObjects]
    public class KeyboardHandlerEditor : UnityEditor.Editor
    {
        private KeyboardHandler instance;

        void OnEnable()
        {
            instance = (KeyboardHandler)target;
        }

        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("Build keyboard"))
            {
                instance.BuildKeyboard();
            }
            DrawDefaultInspector();

        }

    }
#endif
}
