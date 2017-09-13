using UnityEngine;
using System.Collections;
using System;

/// <summary>
/// This class represents the button that brings up the menu for coloring by attributes.
/// </summary>
public class AttributeMenuButton : StationaryButton
{
    private GameObject attributeMenu;
    private GameObject buttons;

    private void Start()
    {
        attributeMenu = referenceManager.attributeSubMenu.gameObject;
        buttons = referenceManager.leftButtons;
    }

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
            descriptionText.text = "";
            attributeMenu.SetActive(true);
            buttons.SetActive(false);
        }
    }

}

