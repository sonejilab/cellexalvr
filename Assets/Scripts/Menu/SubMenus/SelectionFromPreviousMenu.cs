using CellexalVR.General;
using CellexalVR.Menu;
using CellexalVR.Menu.Buttons.Selection;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CellexalVR.Menu.Buttons;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace CellexalVR.Menu.SubMenus
{
    /// <summary>
    /// Represents the sub menu that pops up when the <see cref="ColorByIndexButton"/> is pressed.
    /// </summary>
    public class SelectionFromPreviousMenu : SubMenu
    {
        [FormerlySerializedAs("buttonPrefab")] public GameObject selectionButtonPrefab;
        public GameObject annotationButtonPrefab;
        [HideInInspector] public List<SelectionFromPreviousButton> prevSelectionButtons = new List<SelectionFromPreviousButton>();
        [HideInInspector] public List<SelectAnnotationButton> prevAnnotationButtons = new List<SelectAnnotationButton>();

        private MenuToggler menuToggler;

        // hard coded positions :)
        private Vector3 selectionButtonPos = new Vector3(-.39f, .77f, .282f);
        private readonly Vector3 selectionButtonPosInc = new Vector3(.25f, 0, 0);
        private readonly Vector3 selectionButtonPosNewRowInc = new Vector3(0, 0, -.15f);
        private Vector3 annotationButtonPos = new Vector3(-.39f, .77f, -.20f);
        private readonly Vector3 annotationButtonPosInc = new Vector3(.25f, 0, 0);
        private readonly Vector3 annotationButtonPosNewRowInc = new Vector3(0, 0, -.15f);
        // private List<GameObject> selectionButtons = new List<GameObject>();
        // private List<GameObject> annotationButtons = new List<GameObject>();

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        protected override void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            menuToggler = referenceManager.menuToggler;
            CellexalEvents.GraphsLoaded.AddListener(ReadSelectionFiles);
            CellexalEvents.GraphsLoaded.AddListener(ReadAnnotationFiles);
            base.Start();
        }

        /// <summary>
        /// Read through user folder for previously made or imported selections and create a button for each. 
        /// </summary>
        public void ReadSelectionFiles()
        {
            foreach (SelectionFromPreviousButton button in prevSelectionButtons)
            {
                Destroy(button.gameObject, .1f);
            }

            annotationButtonPos = new Vector3(-.39f, .77f, -.20f);
            selectionButtonPos = new Vector3(-.39f, .77f, .282f);
            prevSelectionButtons.Clear();
            string path = CellexalUser.UserSpecificFolder;
            string[] files = Directory.GetFiles(path, "selection*.txt");
            int i = 0;
            foreach (string file in files)
            {
                GameObject buttonGameObject = Instantiate(selectionButtonPrefab, transform);
                buttonGameObject.SetActive(true);
                buttonGameObject.transform.localPosition = selectionButtonPos;

                SelectionFromPreviousButton button = buttonGameObject.GetComponent<SelectionFromPreviousButton>();
                prevSelectionButtons.Add(button);
                button.Path = file;
                string[] words = file.Split('\\');
                button.buttonDescription.text = words[words.Length - 1];

                // selectionButtons.Add(buttonGameObject);
                if ((i + 1) % 4 == 0)
                {
                    selectionButtonPos -= selectionButtonPosInc * 3;
                    selectionButtonPos += selectionButtonPosNewRowInc;
                }
                else
                {
                    selectionButtonPos += selectionButtonPosInc;
                }

                i++;
            }
        }

        /// <summary>
        /// Read through user folder previously made or imported annotation files and create a button on the menu for each.
        /// </summary>
        public void ReadAnnotationFiles()
        {
            foreach (SelectAnnotationButton button in prevAnnotationButtons)
            {
                Destroy(button.gameObject, .1f);
            }

            annotationButtonPos = new Vector3(-.39f, .77f, -.20f);
            prevAnnotationButtons.Clear();
            string path = Path.Combine(CellexalUser.UserSpecificFolder, "AnnotatedSelections");
            if (!Directory.Exists(path)) return;
            string[] files = Directory.GetFiles(path, "*.txt");
            int i = 0;
            foreach (string file in files)
            {
                GameObject buttonGameObject = Instantiate(annotationButtonPrefab, transform);
                buttonGameObject.SetActive(true);
                buttonGameObject.transform.localPosition = annotationButtonPos;

                SelectAnnotationButton button = buttonGameObject.GetComponent<SelectAnnotationButton>();
                prevAnnotationButtons.Add(button);
                button.Path = file;
                string[] words = file.Split('\\');
                button.buttonDescription.text = words[words.Length - 1];
                // annotationButtons.Add(buttonGameObject);
                if ((i + 1) % 4 == 0)
                {
                    annotationButtonPos -= annotationButtonPosInc * 3;
                    annotationButtonPos += annotationButtonPosNewRowInc;
                }
                else
                {
                    annotationButtonPos += annotationButtonPosInc;
                }

                i++;
            }
        }

        public SelectAnnotationButton FindAnnotationButton(string path)
        {
            foreach (SelectAnnotationButton button in prevAnnotationButtons)
            {
                if (!button.Path.Equals(path)) continue;
                return button;
            }

            return null;
        }

        public SelectionFromPreviousButton FindSelectionButton(string path)
        {
            foreach (SelectionFromPreviousButton button in prevSelectionButtons)
            {
                if (!button.Path.Equals(path)) continue;
                return button;
            }

            return null;
        }

        /// <summary>
        /// Creates the buttons for selecting a previous selection. Used when the selections are read from files.
        /// </summary>
        /// <param name="graphNames"> An array with the names of the graphs. </param>
        /// <param name="names"> An array with the names of the selections. </param>
        /// <param name="selectionCellNames"> An array of arrays with the names of the cells in each selection. </param>
        /// <param name="selectionGroups"> An array of arrays with the groups that each cell in each selection belong to. </param>
        // public void SelectionFromPreviousButton(string[] graphNames, string[] names, string[][] selectionCellNames,
        //     int[][] selectionGroups, Dictionary<int, Color>[] groupingColors)
        // {
        //     foreach (GameObject button in selectionButtons)
        //     {
        //         // wait 0.1 seconds so we are out of the loop before we start destroying stuff
        //         Destroy(button.gameObject, .1f);
        //         selectionButtonPos = new Vector3(-.39f, .77f, .282f);
        //     }
        //
        //     for (int i = 0; i < names.Length; ++i)
        //     {
        //         string name = names[i];
        //
        //         var buttonGameObject = Instantiate(selectionButtonPrefab, transform);
        //         buttonGameObject.SetActive(true);
        //         if (!menuToggler)
        //         {
        //             menuToggler = referenceManager.menuToggler;
        //         }
        //
        //         //menuToggler.AddGameObjectToActivate(buttonGameObject, gameObject);
        //         //menuToggler.AddGameObjectToActivate(buttonGameObject.transform.GetChild(0).gameObject, gameObject);
        //         buttonGameObject.transform.localPosition = selectionButtonPos;
        //
        //         var button = buttonGameObject.GetComponent<SelectionFromPreviousButton>();
        //         button.SetSelection(graphNames[i], name, selectionCellNames[i], selectionGroups[i], groupingColors[i]);
        //         selectionButtons.Add(buttonGameObject);
        //
        //         // position the buttons in a 4 column grid.
        //         if ((i + 1) % 4 == 0)
        //         {
        //             selectionButtonPos -= selectionButtonPosInc * 3;
        //             selectionButtonPos += selectionButtonPosNewRowInc;
        //         }
        //         else
        //         {
        //             selectionButtonPos += selectionButtonPosInc;
        //         }
        //     }
        // }

        /// <summary>
        /// Adds one more button to the menu. Used when a new selection is made, after the buttons created from the information in the files have been created.
        /// </summary>
        /// <param name="selectedCells"></param>
        //internal void CreateButton(List<GraphPoint> selectedCells)
        //{
        //    string[] cellnames = new string[selectedCells.Count];
        //    int[] cellgroups = new int[selectedCells.Count];
        //    Dictionary<int, Color> colors = new Dictionary<int, Color>();

        //    for (int i = 0; i < selectedCells.Count; ++i)
        //    {
        //        GraphPoint gp = selectedCells[i];
        //        cellnames[i] = gp.Label;
        //        cellgroups[i] = gp.CurrentGroup;
        //        colors[gp.CurrentGroup] = gp.Material.color;
        //    }
        //    string name = "." + (buttons.Count + 1) + "\n" + colors.Count + "\n" + cellnames.Length;

        //    var buttonGameObject = Instantiate(buttonPrefab, transform);
        //    buttonGameObject.SetActive(true);
        //    menuToggler.AddGameObjectToActivate(buttonGameObject, gameObject);
        //    menuToggler.AddGameObjectToActivate(buttonGameObject.transform.GetChild(0).gameObject, gameObject);
        //    buttonGameObject.transform.localPosition = buttonPos;

        //    var button = buttonGameObject.GetComponent<SelectionFromPreviousButton>();
        //    button.SetSelection(selectedCells[0].GraphName, name, cellnames, cellgroups, colors);
        //    buttons.Add(buttonGameObject);

        //    if (buttons.Count % 4 == 0)
        //    {
        //        buttonPos -= buttonPosInc * 3;
        //        buttonPos += buttonPosNewRowInc;
        //    }
        //    else
        //    {
        //        buttonPos += buttonPosInc;
        //    }

        //}
    }
}