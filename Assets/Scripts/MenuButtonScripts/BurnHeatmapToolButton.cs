///<summary>
/// Represents a button used for toggling the burning heatmap tool.
///</summary>
public class BurnHeatmapToolButton : CellexalButton
{
    private ControllerModelSwitcher controllerModelSwitcher;

    private void Start()
    {
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
        SetButtonActivated(false);
        CellexalEvents.GraphsLoaded.AddListener(TurnOn);
        CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    protected override string Description
    {
        get
        {
            return "Burn heatmaps tool";
        }
    }
    protected override void Click()
    {
        if (controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.DeleteTool)
        {
            controllerModelSwitcher.TurnOffActiveTool(true);
        }
        else
        {
            //controllerModelSwitcher.TurnOffActiveTool();
            controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.DeleteTool;
            controllerModelSwitcher.ActivateDesiredTool();
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
