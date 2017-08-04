
/// <summary>
/// This class represents the class that toggles the minimizer tool
/// </summary>
class MinimizerToolButton : StationaryButton
{
    public ControllerModelSwitcher controllerModelSwitcher;
    public MinimizerTool deleteTool;

    protected override string Description
    {
        get { return "Toggle delete tool"; }
    }

    private void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            bool deleteToolActive = controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.Minimizer;
            if (deleteToolActive)
            {
                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Normal;
                //controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Normal);
            }
            else
            {
                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.Minimizer;
                controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Minimizer);
                controllerModelSwitcher.ActivateDesiredTool();
            }
        }
    }
}
