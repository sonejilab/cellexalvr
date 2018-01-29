using System;
using UnityEngine;

/// <summary>
/// This class represents the button that opens the color by gene menu.
/// </summary>
public class ColorByGeneMenuButton : StationaryButton
{

    private GameObject buttons;
    private ColorByGeneMenu colorByGeneMenu;

    protected override string Description
    {
        get
        {
            return "Show menu for calculating\ntop differentially expressed genes";
        }
    }

    void Start()
    {
        buttons = referenceManager.leftButtons;
        colorByGeneMenu = referenceManager.colorByGeneMenu;
        colorByGeneMenu.gameObject.SetActive(true);
        colorByGeneMenu.SetMenuVisible(false);

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
            colorByGeneMenu.SetMenuVisible(true);

            foreach (StationaryButton b in buttons.GetComponentsInChildren<StationaryButton>())
            {
                b.SetButtonActivated(false);
            }
        }
    }
}

