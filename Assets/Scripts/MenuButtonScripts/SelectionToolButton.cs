using System;
using UnityEngine;


///<summary>
/// This class represents a button used for toggling the selection tool.
///</summary>
public class SelectionToolButton : StationaryButton
{

    public Sprite gray;
    public Sprite original;

    private SelectionToolHandler selectionToolHandler;
    private MenuRotator rotator;
    private SelectionToolMenu selectionToolMenu;
    private ControllerModelSwitcher controllerModelSwitcher;
    private bool menuActive = false;
    //private bool buttonsInitialized = false;

    protected override string Description
    {
        get { return "Toggle selection tool"; }
    }
    private void Start()
    {
        selectionToolHandler = referenceManager.selectionToolHandler;
        rotator = referenceManager.menuRotator;
        selectionToolMenu = referenceManager.selectionToolMenu;
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
        SetButtonActivated(false);
        ButtonEvents.GraphsLoaded.AddListener(TurnOn);
        ButtonEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {

            if (controllerModelSwitcher.DesiredModel != ControllerModelSwitcher.Model.SelectionTool)
            {
                standardTexture = gray;
                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.SelectionTool;
                controllerModelSwitcher.ActivateDesiredTool();
            }
            else
            {
                controllerModelSwitcher.TurnOffActiveTool(true);
                standardTexture = original;
            }
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
