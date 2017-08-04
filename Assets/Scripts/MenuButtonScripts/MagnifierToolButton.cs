
/// <summary>
/// This class represents the button that toggles the magnifier tool.
/// </summary>
class MagnifierToolButton : StationaryButton
{
    public ControllerModelSwitcher controllerModelSwitcher;
    public MagnifierTool magnifier;

    protected override string Description
    {
        get { return "Toggle magnifier tool"; }
    }

    private void Update()
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
        }
    }
}
