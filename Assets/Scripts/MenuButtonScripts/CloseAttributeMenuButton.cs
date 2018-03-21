using UnityEngine;
/// <summary>
/// Represents the button that closes the attribute menu.
/// </summary>
public class CloseAttributeMenuButton : CellexalButton
{
    private GameObject attributeMenu;
    private GameObject buttons;

    protected override string Description
    {
        get { return "Close attribute menu"; }
    }

    private void Start()
    {
        attributeMenu = referenceManager.attributeSubMenu.gameObject;
        buttons = referenceManager.leftButtons;
    }

    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            spriteRenderer.sprite = standardTexture;
            controllerInside = false;
            attributeMenu.SetActive(false);
            buttons.SetActive(true);
        }
    }
}

