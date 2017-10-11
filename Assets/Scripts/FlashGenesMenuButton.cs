
using System;
using UnityEngine;

class FlashGenesMenuButton : StationaryButton
{
    protected override string Description
    {
        get { return "Show menu for flashing genes"; }
    }

    public GameObject menu;


    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            menu.SetActive(true);
        }
    }
}
