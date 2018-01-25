using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents the sub menu that pops up when the ColorByAttributeButton is pressed.
/// </summary>
public class ColorByGeneMenu : MonoBehaviour
{
    public ReferenceManager referenceManager;

    public ColorByGeneButton buttonPrefab;

    private MenuToggler menuToggler;
    // hard coded positions :)
    private Vector3 buttonPos = new Vector3(-.39f, .77f, .282f);
    private Vector3 buttonPosInc = new Vector3(.25f, 0, 0);
    private Vector3 buttonPosNewRowInc = new Vector3(0, 0, -.15f);
    private List<ColorByGeneButton> buttons;

    public void Init()
    {
        buttons = new List<ColorByGeneButton>();
    }

    private void Awake()
    {
        menuToggler = referenceManager.menuToggler;
    }

    /// <summary>
    /// Creates new buttons for coloring by attributes.
    /// </summary>
    /// <param name="genes"> An array of strings that contain the names of the attributes. </param>
    public void CreateGeneButtons(string[] genes)
    {
        if (buttons == null)
        {
            Init();
        }
        foreach (ColorByGeneButton button in buttons)
        {
            // wait 0.1 seconds so we are out of the loop before we start destroying stuff
            Destroy(button.gameObject, .1f);
            buttonPos = new Vector3(-.39f, .77f, .282f);
        }
        buttons.Clear();

        for (int i = 0; i < genes.Length && i < 20; ++i)
        {
            string gene = genes[i];
            ColorByGeneButton newButton = Instantiate(buttonPrefab, transform);
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
            newButton.SetGene(gene);
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
