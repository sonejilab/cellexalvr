using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents the sub menu that pops up when the ColorByAttributeButton is pressed.
/// </summary>
public class AttributeSubMenu : MonoBehaviour
{
    public ReferenceManager referenceManager;

    public ColorByAttributeButton buttonPrefab;

    private MenuToggler menuToggler;
    // hard coded positions :)
    private Vector3 buttonPos = new Vector3(-.39f, .77f, .282f);
    private Vector3 buttonPosInc = new Vector3(.25f, 0, 0);
    private Vector3 buttonPosNewRowInc = new Vector3(0, 0, -.15f);
    private Color[] colors;
    private List<ColorByAttributeButton> buttons;

    public void Init()
    {
        buttons = new List<ColorByAttributeButton>();
        colors = CellExAlConfig.AttributeColors;
    }

    private void Awake()
    {
        menuToggler = referenceManager.menuToggler;
    }

    /// <summary>
    /// Creates new buttons for coloring by attributes.
    /// </summary>
    /// <param name="attributes"> An array of strings that contain the names of the attributes. </param>
    public void CreateAttributeButtons(string[] attributes)
    {
        if (colors == null)
        {
            Init();
        }
        foreach (ColorByAttributeButton button in buttons)
        {
            // wait 0.1 seconds so we are out of the loop before we start destroying stuff
            Destroy(button.gameObject, .1f);
            buttonPos = new Vector3(-.39f, .77f, .282f);
        }
        buttons.Clear();

        for (int i = 0; i < attributes.Length; ++i)
        {
            string attribute = attributes[i];
            ColorByAttributeButton newButton = Instantiate(buttonPrefab, transform);
            newButton.gameObject.SetActive(true);
            if (!menuToggler)
            {
                menuToggler = referenceManager.menuToggler;
            }
            menuToggler.AddGameObjectToActivate(newButton.gameObject, gameObject);
            if (newButton.transform.childCount > 0)
                menuToggler.AddGameObjectToActivate(newButton.transform.GetChild(0).gameObject, gameObject);
            newButton.referenceManager = referenceManager;
            newButton.transform.localPosition = buttonPos;
            newButton.SetAttribute(attribute, colors[i]);
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
