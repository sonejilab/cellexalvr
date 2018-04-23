using UnityEngine;
/// <summary>
/// Represents the button that opens the toggle arcs menu
/// </summary>
public class ArcsMenuButton : CellexalButton
{

    private GameObject buttons;
    private GameObject arcsMenu;

    protected override string Description
    {
        get
        {
            return "Show the toggle arcs menu";
        }
    }

    void Start()
    {
        buttons = referenceManager.backButtons;
        arcsMenu = referenceManager.arcsSubMenu.gameObject;
    }

    protected override void Click()
    {
        spriteRenderer.sprite = standardTexture;
        controllerInside = false;
        arcsMenu.SetActive(true);
        buttons.SetActive(false);
    }

}

