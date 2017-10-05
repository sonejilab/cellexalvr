using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents the sub menu that pops up when the ColorByIndexButton is pressed.
/// </summary>
public class CreateSelectionFromPreviousSelectionMenu : MonoBehaviour
{
    public GameObject buttonPrefab;

    // hard coded positions :)
    private Vector3 buttonPos = new Vector3(-.39f, .77f, .282f);
    private Vector3 buttonPosInc = new Vector3(.25f, 0, 0);
    private Vector3 buttonPosNewRowInc = new Vector3(0, 0, -.15f);
    private List<GameObject> buttons = new List<GameObject>();

    /// <summary>
    /// Creates new buttons for toggling arcs.
    /// </summary>
    /// <param name="networks"> An array of strings that contain the names of the networks. </param>
    public void CreateSelectionFromPreviousSelectionButtons(string[] graphNames, string[] names, string[][] selectionCellNames, Color[][] selectionColors)
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
            buttonGameObject.transform.localPosition = buttonPos;

            var button = buttonGameObject.GetComponent<CreateSelectionFromPreviousButton>();
            button.SetSelection(graphNames[i], name, selectionCellNames[i], selectionColors[i]);
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

    internal void CreateButton(List<GraphPoint> selectedCells)
    {
        string[] cellnames = new string[selectedCells.Count];
        Color[] cellcolors = new Color[selectedCells.Count];
        Dictionary<Color, bool> colors = new Dictionary<Color, bool>();

        for (int i = 0; i < selectedCells.Count; ++i)
        {
            GraphPoint gp = selectedCells[i];
            cellnames[i] = gp.Label;
            cellcolors[i] = gp.Color;
            colors[gp.Color] = true;
        }
        string name = "." + (buttons.Count + 1) + "\n" + colors.Count + "\n" + cellnames.Length;

        var buttonGameObject = Instantiate(buttonPrefab, transform);
        buttonGameObject.SetActive(true);
        buttonGameObject.transform.localPosition = buttonPos;

        var button = buttonGameObject.GetComponent<CreateSelectionFromPreviousButton>();
        button.SetSelection(selectedCells[0].GraphName, name, cellnames, cellcolors);
        buttons.Add(buttonGameObject);

        if (buttons.Count % 4 == 0)
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
