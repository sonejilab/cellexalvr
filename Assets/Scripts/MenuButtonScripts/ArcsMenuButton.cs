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

    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            spriteRenderer.sprite = standardTexture;
            controllerInside = false;
            arcsMenu.SetActive(true);
            buttons.SetActive(false);
        }
    }
}

