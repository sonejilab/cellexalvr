using UnityEngine;
using System.Collections;
using System;

public class ArcsMenuButton : StationaryButton
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

