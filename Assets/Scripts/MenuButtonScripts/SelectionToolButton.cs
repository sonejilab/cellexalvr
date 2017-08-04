using System;
using UnityEngine;


///<summary>
/// This class represents a button used for toggling the selection tool.
///</summary>
public class SelectionToolButton : StationaryButton
{
    public SelectionToolHandler selectionToolHandler;
    public MenuRotator rotator;
    public SelectionToolMenu selectionToolMenu;
    public ControllerModelSwitcher controllerModelSwitcher;

    private bool menuActive = false;
    private bool buttonsInitialized = false;

    protected override string Description
    {
        get
        {
            return "Toggle selection tool";
        }
    }


    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            bool magnifierActive = controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.Magnifier;
            if (magnifierActive)
            {
                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Normal;
            }
            else
            {
                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Magnifier;
                controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Magnifier);
                controllerModelSwitcher.ActivateDesiredTool();
            }
            menuActive = !selectionToolHandler.IsSelectionToolEnabled();
            selectionToolMenu.gameObject.SetActive(menuActive);
            if (menuActive && rotator.SideFacingPlayer == MenuRotator.Rotation.Front)
            {
                rotator.RotateLeft();
            }
            if (!buttonsInitialized)
            {
                selectionToolMenu.InitializeButtons();
                buttonsInitialized = true;
            }
        }
    }
}
