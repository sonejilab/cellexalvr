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

    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            spriteRenderer.sprite = standardTexture;
            controllerInside = false;
            descriptionText.text = "";
            attributeMenu.SetActive(true);
            buttons.SetActive(false);
        }
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
