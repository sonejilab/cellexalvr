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
	public Sprite gray;
	public Sprite original;

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
			if (selectionToolMenu.gameObject.activeSelf) {
				standardTexture = gray;
			} else {
				standardTexture = original;
			}
            selectionToolHandler.SetSelectionToolEnabled(menuActive, true);
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
