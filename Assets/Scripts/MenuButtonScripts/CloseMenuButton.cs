using UnityEngine;

public class CloseMenuButton : StationaryButton
{
    public GameObject buttonsToActivate;
    public GameObject menuToClose;

    public bool deactivateMenu = false;

    protected override string Description
    {
        get
        {
            return "Close menu";
        }
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
            if (deactivateMenu)
            {
                menuToClose.SetActive(false);
            }
            else
            {
                foreach (Renderer r in menuToClose.GetComponentsInChildren<Renderer>())
                    r.enabled = false;
                foreach (Collider c in menuToClose.GetComponentsInChildren<Collider>())
                    c.enabled = false;
            }
            foreach (StationaryButton b in buttonsToActivate.GetComponentsInChildren<StationaryButton>())
            {
                b.SetButtonActivated(true);
            }
        }
    }
}

