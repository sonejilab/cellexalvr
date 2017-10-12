using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents the sub menu that pops up when the ToggleArcsButton is pressed.
/// </summary>
public class ToggleArcsSubMenu : MenuWithTabs
{
    public GameObject buttonPrefab;
    public Tab tabPrefab;
    // hard coded positions :)
    private Vector3 buttonPos = new Vector3(-0.3958f, 0.59f, 0.2688f);
    private Vector3 buttonPosInc = new Vector3(0.25f, 0, 0);
    private Vector3 buttonPosNewRowInc = new Vector3(0, 0, -0.15f);

    private Color[] colors;

    /// <summary>
    /// Initializes the arc menu.
    /// </summary>
    public void Init()
    {
        // TODO CELLEXAL: come up with some more colors
        colors = new Color[22];
        colors[0] = new Color(1, 0, 0);     // red
        colors[1] = new Color(0, 0, 1);     // blue
        colors[2] = new Color(0, 1, 0);     // green
        colors[3] = new Color(1, 1, 0);     // yellow
        colors[4] = new Color(0, 1, 1);     // cyan
        colors[5] = new Color(1, 0, 1);     // magenta
        colors[6] = new Color(1f, 153f / 255f, 204f / 255f);     // pink
        colors[7] = new Color(0.6f, 1, 0.6f);     // lime green
        colors[8] = new Color(0.4f, 0.2f, 1);     // brown
        colors[9] = new Color(1, 0.6f, 0.2f);     // orange
        colors[10] = new Color(0.87f, 8f, 0.47f);     // some ugly sand color
        colors[11] = new Color(0.3f, 0.3f, 0.3f);     // grey
        colors[12] = new Color(0.18f, 0.69f, 0.54f);     // turquoise
        colors[13] = new Color(0.84f, 0.36f, 0.15f);     // red panda red
        colors[14] = new Color(0, 1, 1);     // cyan
        colors[15] = new Color(1, 0, 1);     // magenta
        colors[16] = new Color(1f, 153f / 255f, 204f / 255f);     // pink
        colors[17] = new Color(0.6f, 1, 0.6f);     // lime green
        colors[18] = new Color(0.4f, 0.2f, 1);     // brown
        colors[19] = new Color(1, 0.6f, 0.2f);     // orange
        colors[20] = new Color(0.87f, 8f, 0.47f);     // some ugly sand color
        colors[21] = new Color(0.3f, 0.3f, 0.3f);     // grey
        // gameObject.SetActive(false);
    }

    /// <summary>
    /// Creates new buttons for toggling arcs.
    /// </summary>
    /// <param name="networks"> An array containing the networks. </param>
    public void CreateToggleArcsButtons(NetworkCenter[] networks)
    {
        if (networks.Length == 1)
        {
            CellExAlLog.Log("ERROR: Tried to create buttons of a network handler with zero network centers.");
            return;
        }
        TurnOffAllTabs();
        var newTab = AddTab(tabPrefab);
        // The prefab contains some buttons that needs some variables set.
        ArcsTabButton tabButton = newTab.GetComponentInChildren<ArcsTabButton>();
        tabButton.referenceManager = referenceManager;
        tabButton.tab = newTab;
        tabButton.Handler = networks[0].Handler;

        //newTab.tab = newTab.transform.parent.gameObject;
        if (colors == null)
        {
            Init();
        }
        //foreach (GameObject button in buttons)
        //{
        //    // wait 0.1 seconds so we are out of the loop before we start destroying stuff
        //    Destroy(button.gameObject, .1f);
        //    buttonPos = new Vector3(-.39f, .77f, .282f);
        //}

        var toggleAllArcsButtonsInPrefab = newTab.GetComponentsInChildren<ToggleAllArcsButton>();
        foreach (ToggleAllArcsButton b in toggleAllArcsButtonsInPrefab)
        {
            b.SetNetworks(networks);
        }
        var toggleAllCombindedArcsInPrefab = newTab.GetComponentsInChildren<ToggleAllCombinedArcsButton>();
        foreach (ToggleAllCombinedArcsButton b in toggleAllCombindedArcsInPrefab)
        {
            b.SetNetworks(networks);
        }

        Vector3 buttonPosOrigin = buttonPos;
        for (int i = 0; i < networks.Length; ++i)
        {
            var network = networks[i];
            var newButton = Instantiate(buttonPrefab, newTab.transform);
            newButton.GetComponent<Renderer>().material.color = network.GetComponent<Renderer>().material.color;
            var toggleArcButtonList = newButton.GetComponentsInChildren<ToggleArcsButton>();
            newButton.transform.localPosition = buttonPos;
            newButton.gameObject.SetActive(true);
            foreach (var toggleArcButton in toggleArcButtonList)
            {
                toggleArcButton.combinedNetworksButton = newTab.GetComponentInChildren<ToggleAllCombinedArcsButton>();
                toggleArcButton.SetNetwork(network);
            }
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
        buttonPos = buttonPosOrigin;
    }
}
