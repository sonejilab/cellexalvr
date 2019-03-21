using UnityEngine;
using System.Collections.Generic;
using System;
using CellexalVR.General;
using UnityEngine.Events;



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
        public AutoCompleteList autoCompleteList;
        public TMPro.TextMeshPro output;
        public float angle = 60f;
        public float distance = 8f;
        public float autoCompleteListSpacing = 1.28f;
        public enum Layout { Uppercase, Lowercase, Special }
        public Layout currentLayout = Layout.Lowercase;
        public string placeholder = "Enter a gene name";
        public KeyboardEvent OnEdit;
        public KeyboardEvent OnEnter;

        private bool keyLayoutUppercase = false;
        private KeyboardPanel[] sortedKeys;


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
            Clear();
            SwitchLayout(Layout.Lowercase);
            GatherKeys();
        }

        /// <summary>
        /// Sets the materials on all keys. See <see cref="ClickablePanel.SetMaterials(Material, Material, Material)"/>.
        /// </summary>
        public void SetMaterials(Material keyNormalMaterial, Material keyHighlightMaterial, Material keyPressedMaterial)
        {
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
            sortedKeys = keysParentObject.GetComponentsInChildren<KeyboardPanel>();
            // sort by position, y first then x
            Array.Sort(sortedKeys, (KeyboardPanel a, KeyboardPanel b) => a.position.y == b.position.y ? a.position.x - b.position.x : a.position.y - b.position.y);
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
                sortedKeys[i].Text = layoutToSwitchTo[i];
            }
            currentLayout = layout;
        }

        /// <summary>
        /// Colors all graphs based on what was typed.
        /// </summary>
        public void SubmitOutput()
        {
            if (OnEnter != null)
            {
                OnEnter.Invoke(Text());
            }
            //referenceManager.cellManager.ColorGraphsByGene(output.text);
            //referenceManager.previousSearchesList.AddEntry(output.text, Extensions.Definitions.Measurement.GENE, referenceManager.graphManager.GeneExpressionColoringMethod);
            //referenceManager.gameManager.InformColorGraphsByGene(output.text);
            Clear();
        }

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
            if (!gameObject.activeInHierarchy || Event.current != null && Event.current.type == EventType.Repaint)
                return;

            if (referenceManager == null)
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<General.ReferenceManager>();
            }
            SwitchLayout(currentLayout);
            output.text = placeholder;

            // place all keys in a curve
            float halfAngle = angle / 2f;
            KeyboardPanel[] keys = keysParentObject.GetComponentsInChildren<KeyboardPanel>();
            foreach (KeyboardPanel panel in keys)
            {
                float keyAngleDegrees = halfAngle * (9f - panel.position.x) / 9f;
                float keyAngleRadians = keyAngleDegrees / 180 * Mathf.PI;
                float yPos = 3 - panel.position.y + 0.5f;
                Transform key = panel.transform.parent;
                Vector3 position = new Vector3(Mathf.Cos(keyAngleRadians), 0, Mathf.Sin(keyAngleRadians)) * distance;
                position += new Vector3(-distance, yPos, 0);
                key.transform.localPosition = position;
                key.localRotation = Quaternion.Euler(0f, 90 - keyAngleDegrees, 0f);
                if (panel.position.y == 3)
                {
                    MeshFilter meshFilter = panel.GetComponentInChildren<MeshFilter>();
                    if (meshFilter.sharedMesh == null)
                    {
                        meshFilter.sharedMesh = new Mesh();
                    }
                    meshFilter.sharedMesh = CurveSpaceBar(meshFilter.sharedMesh, angle / (3f * 180) * Mathf.PI, distance);
                }
            }
            if (autoCompleteList == null)
            {
                return;
            }
            List<ClickableTextPanel> autoCompletePanels = autoCompleteList.listNodes;
            int length = autoCompletePanels.Count;
            float halfLength = (length + 1) / 2f;
            for (int i = 0; i < length; ++i)
            {
                ClickableTextPanel panel = autoCompletePanels[i];
                float keyAngleDegrees = halfAngle * (halfLength - (i + 1)) / halfLength * autoCompleteListSpacing;
                float keyAngleRadians = keyAngleDegrees / 180 * Mathf.PI;
                panel.transform.localPosition = new Vector3(Mathf.Cos(keyAngleRadians), 0, Mathf.Sin(keyAngleRadians)) * distance;
                panel.transform.localRotation = Quaternion.Euler(0f, 90 - keyAngleDegrees, 0f);
                panel.transform.Translate(-distance, 0f, 0f, keysParentObject.transform);
            }
        }

        /// <summary>
        /// Curves the spacebar mesh. The mesh will be curved along the arc of a circular sector.
        /// </summary>
        /// <param name="mesh">The mesh to curve.</param>
        /// <param name="angle">The angle of the circular sector.</param>
        /// <param name="distance">The radius of the circular sector.</param>
        private Mesh CurveSpaceBar(Mesh mesh, float angle, float distance)
        {
            Vector3[] verts = mesh.vertices;
            int[] tris = mesh.triangles;
            Vector3 yOffset = new Vector3(0, 0.5f, 0);
            Vector3 xOffset = new Vector3(0, 0, -distance);
            for (int i = 0; i < verts.Length; i += 2)
            {
                float vertAngle = angle * (6 - i) / 6f;
                float cos = Mathf.Cos(vertAngle);
                float sin = Mathf.Sin(vertAngle);
                Vector3 vertPos1 = new Vector3(sin, 0, cos) * distance + xOffset;
                vertPos1 -= yOffset;
                Vector3 vertPos2 = new Vector3(sin, 0, cos) * distance + xOffset;
                vertPos2 += yOffset;
                verts[i] = vertPos1;
                verts[i + 1] = vertPos2;
            }

            for (int i = 0; i < tris.Length; i += 6)
            {
                int vert = i / 3;
                tris[i] = vert;
                tris[i + 1] = vert + 2;
                tris[i + 2] = vert + 1;
                tris[i + 3] = vert + 1;
                tris[i + 4] = vert + 2;
                tris[i + 5] = vert + 3;
            }
            mesh.vertices = verts;
            mesh.triangles = tris;
            mesh.RecalculateBounds();
            mesh.RecalculateNormals();
            return mesh;
        }

        /// <summary>
        /// Curves the spacebar mesh. The mesh will be curved along the arc of a circular sector.
        /// This function creates a new mesh.
        /// </summary>
        /// <param name="angle">The angle of the circular sector.</param>
        /// <param name="distance">The radius of the circular sector.</param>
        private Mesh CurveMesh(float angle, float distance)
        {
            Mesh mesh = new Mesh();
            mesh.vertices = new Vector3[14];
            mesh.triangles = new int[36];
            return CurveSpaceBar(mesh, angle, distance);
        }
#endif
    }

}
