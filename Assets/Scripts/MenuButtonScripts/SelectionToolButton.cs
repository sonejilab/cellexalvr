using System;
using UnityEngine;


///<summary>
/// This class represents a button used for toggling the selection tool.
///</summary>
public class SelectionToolButton : StationaryButton
{

    private MenuRotator rotator;
    private ControllerModelSwitcher controllerModelSwitcher;
    private bool menuActive = false;
    //private bool buttonsInitialized = false;

    protected override string Description
    {
        get { return "Toggle selection tool"; }
    }
    private void Start()
    {
        rotator = referenceManager.menuRotator;
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
        SetButtonActivated(false);
        CellExAlEvents.GraphsLoaded.AddListener(TurnOn);
        CellExAlEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {

            if (controllerModelSwitcher.DesiredModel != ControllerModelSwitcher.Model.SelectionTool)
            {
                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.SelectionTool;
                controllerModelSwitcher.ActivateDesiredTool();
            }
            else
            {
                controllerModelSwitcher.TurnOffActiveTool(true);
            }
            if (menuActive && rotator.SideFacingPlayer == MenuRotator.Rotation.Front)
            {
                rotator.RotateLeft();
            }
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
