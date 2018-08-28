using UnityEngine;

/// <summary>
/// This class represents the sub menu that pops up when the ToggleArcsButton is pressed.
/// </summary>
public class ToggleArcsSubMenu : MenuWithTabs
{
    public GameObject buttonPrefab;
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
        colors = CellexalConfig.SelectionToolColors;
    }

    /// <summary>
    /// Creates new buttons for toggling arcs.
    /// </summary>
    /// <param name="networks"> An array containing the networks. </param>
    public void CreateToggleArcsButtons(NetworkCenter[] networks)
    {
        if (networks.Length == 1)
        {
            CellexalLog.Log("ERROR: Tried to create buttons of a network handler with zero network centers.");
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
            newButton.name = "ArcButton" + i;
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
        TurnOffAllTabs();
        newTab.SetTabActive(GetComponent<Renderer>().enabled);
        buttonPos = buttonPosOrigin;
    }
}
