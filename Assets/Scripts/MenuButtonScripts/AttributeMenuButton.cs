using UnityEngine;
using System.Collections;
using System;

public class AttributeMenuButton : StationaryButton
{

    public GameObject attributeMenu;
    public GameObject buttons;

    protected override string Description
    {
        get
        {
            return "Show menu for coloring by attribute";
        }
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            spriteRenderer.sprite = standardTexture;
            controllerInside = false;
            attributeMenu.SetActive(true);
            buttons.SetActive(false);
        }
    }

}

