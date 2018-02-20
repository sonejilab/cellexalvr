///<summary>
/// Represents a button used for toggling the burning heatmap tool.
///</summary>
public class BurnHeatmapToolButton : StationaryButton
{
    private ControllerModelSwitcher controllerModelSwitcher;

    private void Start()
    {
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
        SetButtonActivated(false);
        CellExAlEvents.HeatmapCreated.AddListener(TurnOn);
        CellExAlEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    protected override string Description
    {
        get
        {
            return "Burn heatmaps tool";
        }
    }
    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            if (controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.HeatmapDeleteTool)
            {
                controllerModelSwitcher.TurnOffActiveTool(true);
            }
            else
            {
                //controllerModelSwitcher.TurnOffActiveTool();
                controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.HeatmapDeleteTool;
                controllerModelSwitcher.ActivateDesiredTool();
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
