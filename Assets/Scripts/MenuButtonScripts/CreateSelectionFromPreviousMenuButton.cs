using UnityEngine;

class CreateSelectionFromPreviousMenuButton : StationaryButton
{
    public GameObject menu;
    public GameObject buttons;

    protected override string Description
    {
        get { return "Show menu for selection a previous selection"; }
    }

    private void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            buttons.SetActive(false);
            menu.SetActive(true);
            controllerInside = false;

        }
    }
}
