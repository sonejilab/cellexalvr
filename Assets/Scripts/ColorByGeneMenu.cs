using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the sub menu that pops up when the <see cref="ColorByGeneMenuButton"/> is pressed.
/// </summary>
public class ColorByGeneMenu : MonoBehaviour
{
    public ReferenceManager referenceManager;

    public ColorByGeneButton buttonPrefab;
    public GameObject loadingText;

    private MenuToggler menuToggler;
    // hard coded positions :)
    private Vector3 buttonPos = new Vector3(-0.40f, 1.3f, 0.135f);
    private Vector3 buttonPosInc = new Vector3(0.20f, 0, 0);
    private Vector3 buttonPosNewRowInc = new Vector3(0, 0, -0.15f);
    private List<ColorByGeneButton> buttons;

    public void Init()
    {
        buttons = new List<ColorByGeneButton>();
    }

    private void Awake()
    {
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        menuToggler = referenceManager.menuToggler;
        CellexalEvents.QueryTopGenesStarted.AddListener(ShowLoadingText);
        CellexalEvents.QueryTopGenesFinished.AddListener(HideLoadingText);
    }

    /// <summary>
    /// Sets the menu to be visible or invisible
    /// </summary>
    /// <param name="visible"> True for making the menu visible, false for invisible. </param>
    public void SetMenuVisible(bool visible)
    {
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
        {
            r.enabled = visible;
        }
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = visible;
        }
    }

    /// <summary>
    /// Creates new buttons for coloring by attributes.
    /// </summary>
    /// <param name="genes"> An array of strings that contain the names of the attributes. </param>
    public void CreateGeneButtons(string[] genes, float[] values)
    {
        if (buttons == null)
        {
            Init();
        }
        foreach (ColorByGeneButton button in buttons)
        {
            // wait 0.1 seconds so we are out of the loop before we start destroying stuff
            Destroy(button.gameObject, .1f);
            buttonPos = new Vector3(-0.40f, 1.3f, 0.135f);
        }
        buttons.Clear();

        bool menuOn = GetComponent<Renderer>().enabled;

        for (int i = 0; i < genes.Length && i < 20; ++i)
        {
            // first take the top 10, then the bottom 10
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
            newButton.SetGene(gene, values[i]);
            buttons.Add(newButton);
            // position the buttons in a 5 column grid.
            if ((i + 1) % 5 == 0)
            {
                buttonPos -= buttonPosInc * 4;
                buttonPos += buttonPosNewRowInc;
            }
            else
            {
                buttonPos += buttonPosInc;
            }

        }
    }

    private void ShowLoadingText()
    {
        loadingText.SetActive(true);
    }

    private void HideLoadingText()
    {
        loadingText.SetActive(false);
    }
}
