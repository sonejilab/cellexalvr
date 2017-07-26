using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents the sub menu that pops up when the ToggleArcsButton is pressed.
/// </summary>
public class ToggleArcsSubMenu : MonoBehaviour
{

    public GameObject buttonPrefab;
    public ToggleAllArcsButton toggleAllArcsOnButton;
    public ToggleAllArcsButton toggleAllArcsOffButton;
    // hard coded positions :)
    private Vector3 buttonPos = new Vector3(-.39f, .77f, .282f);
    private Vector3 buttonPosInc = new Vector3(.25f, 0, 0);
    private Vector3 buttonPosNewRowInc = new Vector3(0, 0, -.15f);
    private Color[] colors;
    private List<GameObject> buttons;

    public void Init()
    {
        // TODO come up with some more colors
        buttons = new List<GameObject>();
        colors = new Color[22];
        colors[0] = new Color(1, 0, 0);     // red
        colors[1] = new Color(0, 0, 1);     // blue
        colors[2] = new Color(0, 1, 0);     // green
        colors[3] = new Color(1, 1, 0);     // yellow
        colors[4] = new Color(0, 1, 1);     // cyan
        colors[5] = new Color(1, 0, 1);     // magenta
        colors[6] = new Color(1f, 153f / 255f, 204f / 255f);     // pink
        colors[7] = new Color(.6f, 1, .6f);     // lime green
        colors[8] = new Color(.4f, .2f, 1);     // brown
        colors[9] = new Color(1, .6f, .2f);     // orange
        colors[10] = new Color(.87f, 8f, .47f);     // some ugly sand color
        colors[11] = new Color(.3f, .3f, .3f);     // grey
        colors[12] = new Color(.18f, .69f, .54f);     // turquoise
        colors[13] = new Color(.84f, .36f, .15f);     // red panda red
        colors[14] = new Color(0, 1, 1);     // cyan
        colors[15] = new Color(1, 0, 1);     // magenta
        colors[16] = new Color(1f, 153f / 255f, 204f / 255f);     // pink
        colors[17] = new Color(.6f, 1, .6f);     // lime green
        colors[18] = new Color(.4f, .2f, 1);     // brown
        colors[19] = new Color(1, .6f, .2f);     // orange
        colors[20] = new Color(.87f, 8f, .47f);     // some ugly sand color
        colors[21] = new Color(.3f, .3f, .3f);     // grey
        // gameObject.SetActive(false);
    }

    /// <summary>
    /// Creates new buttons for toggling arcs.
    /// </summary>
    /// <param name="networks"> An array of strings that contain the names of the networks. </param>
    public void CreateToggleArcsButtons(NetworkCenter[] networks)
    {
        if (colors == null)
        {
            Init();
        }
        foreach (GameObject button in buttons)
        {
            // wait 0.1 seconds so we are out of the loop before we start destroying stuff
            Destroy(button.gameObject, .1f);
            buttonPos = new Vector3(-.39f, .77f, .282f);
        }
        toggleAllArcsOffButton.SetNetworks(networks);
        toggleAllArcsOnButton.SetNetworks(networks);
        for (int i = 0; i < networks.Length; ++i)
        {
            var network = networks[i];
            var newButton = Instantiate(buttonPrefab, transform);
            newButton.GetComponent<Renderer>().material.color = network.GetComponent<Renderer>().material.color;
            var toggleArcButtonList = newButton.GetComponentsInChildren<ToggleArcsButton>();
            newButton.transform.localPosition = buttonPos;
            newButton.gameObject.SetActive(true);
            foreach (var toggleArcButton in toggleArcButtonList)
            {
                toggleArcButton.SetNetwork(network);
            }
            buttons.Add(newButton);
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
}
