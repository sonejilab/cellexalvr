using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the sub menu that pops up when the <see cref="ColorByIndexButton"/> is pressed.
/// </summary>
public class ColorByIndexMenu : MonoBehaviour
{
    public ReferenceManager referenceManager;

    public GameObject buttonPrefab;

    private MenuToggler menuToggler;
    // hard coded positions :)
    private Vector3 buttonPos = new Vector3(-.39f, .77f, .282f);
    private Vector3 buttonPosInc = new Vector3(.25f, 0, 0);
    private Vector3 buttonPosNewRowInc = new Vector3(0, 0, -.15f);
    private List<GameObject> buttons = new List<GameObject>();

    private void Start()
    {
        menuToggler = referenceManager.menuToggler;
    }

    /// <summary>
    /// Creates new buttons for toggling arcs.
    /// </summary>
    /// <param name="networks"> An array of strings that contain the names of the networks. </param>
    public void CreateColorByIndexButtons(string[] names)
    {

        foreach (GameObject button in buttons)
        {
            // wait 0.1 seconds so we are out of the loop before we start destroying stuff
            Destroy(button.gameObject, .1f);
            buttonPos = new Vector3(-.39f, .77f, .282f);
        }
        for (int i = 0; i < names.Length; ++i)
        {
            var name = names[i];
            var newButton = Instantiate(buttonPrefab, transform);
            var colorByIndexButtonList = newButton.GetComponentsInChildren<ColorByIndexButton>();
            newButton.transform.localPosition = buttonPos;
            newButton.gameObject.SetActive(true);
            if (!menuToggler)
            {
                menuToggler = referenceManager.menuToggler;
            }
            menuToggler.AddGameObjectToActivate(newButton, gameObject);
            if (newButton.transform.childCount > 0)
                menuToggler.AddGameObjectToActivate(newButton.transform.GetChild(0).gameObject, gameObject);
            foreach (var toggleArcButton in colorByIndexButtonList)
            {
                toggleArcButton.SetIndex(name);
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
