using UnityEngine;

/// <summary>
/// Represents the button that brings up the menu for coloring by attributes.
/// </summary>
public class AttributeMenuButton : CellexalButton
{
    private GameObject attributeMenu;
    private GameObject buttons;

    protected override string Description
    {
        get { return "Show menu for coloring by attribute"; }
    }

    private void Start()
    {
        attributeMenu = referenceManager.attributeSubMenu.gameObject;
        buttons = referenceManager.leftButtons;
        SetButtonActivated(false);
        CellexalEvents.GraphsLoaded.AddListener(TurnOn);
        CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    protected override void Click()
    {
        spriteRenderer.sprite = standardTexture;
        controllerInside = false;
        descriptionText.text = "";
        attributeMenu.SetActive(true);
        buttons.SetActive(false);

    }

    private void TurnOn()
    {
        SetButtonActivated(true);
    }

    private void TurnOff()
    {
        SetButtonActivated(false);
    }
}
