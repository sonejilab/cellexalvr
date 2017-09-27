using UnityEngine;

public class CloseMenuButton : StationaryButton
{
    public GameObject buttonsToActivate;
    public GameObject menuToClose;

    protected override string Description
    {
        get
        {
            return "Close menu";
        }
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            spriteRenderer.sprite = standardTexture;
            controllerInside = false;
            menuToClose.SetActive(false);
            buttonsToActivate.SetActive(true);
        }
    }
}

