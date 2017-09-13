using UnityEngine;
using System.Collections;
using System;

public class CloseArcsMenuButton : StationaryButton
{
    private GameObject arcsMenu;
    private GameObject buttons;

    protected override string Description
    {
        get
        {
            return "Close attribute menu";
        }
    }

    private void Start()
    {
        arcsMenu = referenceManager.arcsSubMenu.gameObject;
        buttons = referenceManager.backButtons;
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            spriteRenderer.sprite = standardTexture;
            controllerInside = false;
            arcsMenu.SetActive(false);
            buttons.SetActive(true);
        }
    }
}

