using System;
using UnityEngine;

public class ArcsMenuButton : StationaryButton
{

    public GameObject arcsMenu;
    public GameObject buttons;

    protected override string Description
    {
        get
        {
            return "Show menu for toggling arcs between networks";
        }
    }

    void Update()
    {
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

