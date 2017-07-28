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
            menuActive = !selectionToolHandler.IsSelectionToolEnabled();
            selectionToolMenu.gameObject.SetActive(menuActive);
            selectionToolHandler.SetSelectionToolEnabled(menuActive);

            if (menuActive && rotator.rotation == 0)
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
