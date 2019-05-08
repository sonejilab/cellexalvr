using CellexalVR.General;
using CellexalVR.Menu.Buttons.Selection;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the sub menu that pops up when the <see cref="ColorByIndexButton"/> is pressed.
/// </summary>
public class SelectionFromPreviousMenu : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public GameObject buttonPrefab;
    private MenuToggler menuToggler;
    // hard coded positions :)
    private Vector3 buttonPos = new Vector3(-.39f, .77f, .282f);
    private Vector3 buttonPosInc = new Vector3(.25f, 0, 0);
    private Vector3 buttonPosNewRowInc = new Vector3(0, 0, -.15f);
    private List<GameObject> buttons = new List<GameObject>();

    private void OnValidate()
    {
        if (gameObject.scene.IsValid())
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }
    }

    private void Start()
    {
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        menuToggler = referenceManager.menuToggler;
    }

    /// <summary>
    /// Creates the buttons for selecting a previous selection. Used when the selections are read from files.
    /// </summary>
    /// <param name="graphNames"> An array with the names of the graphs. </param>
    /// <param name="names"> An array with the names of the selections. </param>
    /// <param name="selectionCellNames"> An array of arrays with the names of the cells in each selection. </param>
    /// <param name="selectionGroups"> An array of arrays with the groups that each cell in each selection belong to. </param>
    public void SelectionFromPreviousButton(string[] graphNames, string[] names, string[][] selectionCellNames, int[][] selectionGroups, Dictionary<int, Color>[] groupingColors)
    {

        foreach (GameObject button in buttons)
        {
            // wait 0.1 seconds so we are out of the loop before we start destroying stuff
            Destroy(button.gameObject, .1f);
            buttonPos = new Vector3(-.39f, .77f, .282f);
        }
        for (int i = 0; i < names.Length; ++i)
        {
            string name = names[i];

            var buttonGameObject = Instantiate(buttonPrefab, transform);
            buttonGameObject.SetActive(true);
            if (!menuToggler)
            {
                menuToggler = referenceManager.menuToggler;
            }
            menuToggler.AddGameObjectToActivate(buttonGameObject, gameObject);
            menuToggler.AddGameObjectToActivate(buttonGameObject.transform.GetChild(0).gameObject, gameObject);
            buttonGameObject.transform.localPosition = buttonPos;

            var button = buttonGameObject.GetComponent<SelectionFromPreviousButton>();
            button.SetSelection(graphNames[i], name, selectionCellNames[i], selectionGroups[i], groupingColors[i]);
            buttons.Add(buttonGameObject);

            // position the buttons in a 4 column grid.
            if ((i + 1) % 4 == 0)
            {
                buttonPos -= buttonPosInc * 3;
                buttonPos += buttonPosNewRowInc;
            }
            else
            {
                buttonPos += buttonPosInc;
            }
        }
    }

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
