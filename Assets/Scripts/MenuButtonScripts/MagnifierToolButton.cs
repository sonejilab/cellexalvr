using UnityEngine;
/// <summary>
/// This class represents the button that toggles the magnifier tool.
/// </summary>
class MagnifierToolButton : StationaryButton
{
    public ControllerModelSwitcher controllerModelSwitcher;
    public GameObject magnifier;
    public Sprite gray;
    public Sprite original;

    protected override string Description
    {
        get { return "Toggle magnifier tool"; }
    }

    private void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            bool magnifierToolActivated = controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.Magnifier;
            if (magnifierToolActivated)
            {
                controllerModelSwitcher.TurnOffActiveTool(true);
                //controllerModelSwitcher.SwitchToDesiredModel();
                //controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Normal;
            }
            else
            {
                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Magnifier;
                controllerModelSwitcher.ActivateDesiredTool();
            }
        }
    }
}

