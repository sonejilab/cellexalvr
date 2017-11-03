using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolsButton : StationaryButton {

    public Sprite gray;
    public Sprite original;
    
    private MenuRotator rotator;
    private bool menuActive = true;
    //private bool buttonsInitialized = false;

    protected override string Description
    {
        get { return "Tool Menu"; }
    }
    private void Start()
    {
        rotator = referenceManager.menuRotator;
        SetButtonActivated(true);
    }

    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {

            if (menuActive && rotator.SideFacingPlayer == MenuRotator.Rotation.Front)
            {
                rotator.RotateLeft();
            }
            //if (!buttonsInitialized)
            //{
            //    selectionToolMenu.InitializeButtons();
            //    buttonsInitialized = true;
            //}
        }
    }

    private void TurnOn()
    {
        SetButtonActivated(true);
    }

    private void TurnOff()
    {
        SetButtonActivated(false);
    }
}
