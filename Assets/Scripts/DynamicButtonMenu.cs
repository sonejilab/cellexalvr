using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Abstract class that is used for holding multiple <see cref="CellexalButton"/>.
/// </summary>
public abstract class DynamicButtonMenu : MonoBehaviour
{

    public ReferenceManager referenceManager;

    public GameObject buttonPrefab;

    /// <summary>
    /// The list of colors that should be used for the buttons. Leave empty if the buttons' colors should not be changed.
    /// </summary>
    protected abstract Color[] Colors { get; }
    protected List<GameObject> buttons;

    private MenuToggler menuToggler;
    // hard coded positions :)
    private Vector3 buttonPos = new Vector3(-.39f, .77f, .282f);
    private Vector3 buttonPosInc = new Vector3(.25f, 0, 0);
    private Vector3 buttonPosNewRowInc = new Vector3(0, 0, -.15f);

    private void Start()
    {
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        menuToggler = referenceManager.menuToggler;
    }

    /// <summary>
    /// Creates new buttons for coloring by attributes.
    /// </summary>
    /// <param name="names"> An array of strings that contain the names of the attributes. </param>
    public virtual void CreateButtons(string[] names)
    {
        if (buttons == null)
            buttons = new List<GameObject>();
        foreach (var button in buttons)
        {
            // wait 0.1 seconds so we are out of the loop before we start destroying stuff
            Destroy(button.gameObject, .1f);
            buttonPos = new Vector3(-.39f, .77f, .282f);
        }
        buttons.Clear();

        for (int i = 0; i < names.Length; ++i)
        {
            var newButton = Instantiate(buttonPrefab, transform);
            newButton.gameObject.SetActive(true);
            if (!menuToggler)
            {
                menuToggler = referenceManager.menuToggler;
            }
            menuToggler.AddGameObjectToActivate(newButton.gameObject, gameObject);
            if (newButton.transform.childCount > 0)
                menuToggler.AddGameObjectToActivate(newButton.transform.GetChild(0).gameObject, gameObject);
            newButton.transform.localPosition = buttonPos;
            if (i < Colors.Length)
                newButton.GetComponent<Renderer>().material.color = Colors[i];
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
