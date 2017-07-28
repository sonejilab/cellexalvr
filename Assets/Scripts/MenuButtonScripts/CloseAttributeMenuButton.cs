using UnityEngine;
using System.Collections;
using System;

public class CloseAttributeMenuButton : StationaryButton
{
    public GameObject attributeMenu;
    public GameObject buttons;

    protected override string Description
    {
        get
        {
            return "Close attribute menu";
        }
    }


    void Update()
    {
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

