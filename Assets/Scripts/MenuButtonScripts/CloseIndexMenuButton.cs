using UnityEngine;
using System.Collections;
using System;

public class CloseIndexMenuButton : StationaryButton
{
    private GameObject indexMenu;
    private GameObject buttons;

    protected override string Description
    {
        get
        {
            return "Close index menu";
        }
    }

    private void Start()
    {
        indexMenu = referenceManager.indexMenu.gameObject;
        buttons = referenceManager.leftButtons;
    }

    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            spriteRenderer.sprite = standardTexture;
            controllerInside = false;
            indexMenu.SetActive(false);
            buttons.SetActive(true);
        }
    }
}

