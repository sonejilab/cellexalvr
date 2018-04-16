using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the sub menu that pops up when the <see cref="AttributeMenuButton"/> is pressed.
/// </summary>
public class AttributeSubMenu : MenuWithTabs
{
    protected Color[] Colors
    {
        get { return CellexalConfig.AttributeColors; }
    }

    public ColorByAttributeButton buttonPrefab;

    protected List<ColorByAttributeButton> buttons;
    // hard coded positions :)
    private Vector3 buttonPos = new Vector3(-.39f, .77f, .282f);
    private Vector3 buttonPosOriginal = new Vector3(-.39f, .77f, .282f);
    private Vector3 buttonPosInc = new Vector3(.25f, 0, 0);
    private Vector3 buttonPosNewRowInc = new Vector3(0, 0, -.15f);


    /// <summary>
    /// Fill the menu with buttons that will color graphs according to attributes when pressed.
    /// </summary>
    /// <param name="categoriesAndNames">The names of the attributes.</param>
    public void CreateButtons(string[] categoriesAndNames)
    {
        DestroyTabs();

        if (buttons == null)
            buttons = new List<ColorByAttributeButton>();
        foreach (var button in buttons)
        {
            // wait 0.1 seconds so we are out of the loop before we start destroying stuff
            Destroy(button.gameObject, .1f);
        }
        buttonPos = new Vector3(-.39f, .77f, .282f);
        buttons.Clear();
        //TurnOffAllTabs();
        string[] categories = new string[categoriesAndNames.Length];
        string[] names = new string[categoriesAndNames.Length];
        for (int i = 0; i < categoriesAndNames.Length; ++i)
        {
            string[] categoryAndName = categoriesAndNames[i].Split('.');
            categories[i] = categoryAndName[0];
            names[i] = categoryAndName[1];
        }

        Tab newTab = null;

        for (int i = 0, buttonIndex = 0; i < names.Length; ++i, ++buttonIndex)
        {
            // add a new tab if we encounter a new category, or if the current tab is full
            if (buttonIndex % 24 == 0 || i > 0 && categories[i] != categories[i - 1])
            {
                newTab = AddTab(tabPrefab);
                buttonPos = buttonPosOriginal;
                newTab.TabButton.GetComponentInChildren<TextMesh>().text = categories[i];
                buttonIndex = 0;
            }
            var newButton = Instantiate(buttonPrefab, newTab.transform);
            newButton.gameObject.SetActive(true);
            if (!menuToggler)
            {
                menuToggler = referenceManager.menuToggler;
            }
            //menuToggler.AddGameObjectToActivate(newButton.gameObject, gameObject);
            if (newButton.transform.childCount > 0)
                menuToggler.AddGameObjectToActivate(newButton.transform.GetChild(0).gameObject, gameObject);
            newButton.transform.localPosition = buttonPos;
            if (buttonIndex < Colors.Length)
                newButton.GetComponent<Renderer>().material.color = Colors[buttonIndex];
            buttons.Add(newButton);
            // position the buttons in a 4 column grid.
            if ((buttonIndex + 1) % 4 == 0)
            {
                buttonPos -= buttonPosInc * 3;
                buttonPos += buttonPosNewRowInc;
            }
            else
            {
                buttonPos += buttonPosInc;
            }

        }
        // set the names of the attributes after the buttons have been created.
        for (int i = 0; i < buttons.Count; ++i)
        {
            var b = buttons[i];
            b.referenceManager = referenceManager;
            int colorIndex = i % Colors.Length;
            b.SetAttribute(categoriesAndNames[i], names[i], Colors[colorIndex]);
        }
        // turn on one of the tabs
        TurnOffAllTabs();
        newTab.SetTabActive(GetComponent<Renderer>().enabled);

    }
}
