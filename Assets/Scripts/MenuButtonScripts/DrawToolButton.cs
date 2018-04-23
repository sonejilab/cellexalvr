/// <summary>
/// Represents the button that toggles the draw tool.
/// </summary>
public class DrawToolButton : CellexalButton
{
    protected override string Description
    {
        get { return "Toggles the draw tool"; }
    }

    private DrawTool drawTool;
    private ControllerModelSwitcher controllerModelSwitcher;

    private void Start()
    {
        drawTool = referenceManager.drawTool;
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
    }

    protected override void Click()
    {
        bool toolEnabled = controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.DrawTool;
        if (toolEnabled)
        {
            controllerModelSwitcher.TurnOffActiveTool(true);
        }
        else
        {

            controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.DrawTool;
            controllerModelSwitcher.ActivateDesiredTool();
            // Tell the draw tool to skip its next draw because we used the trigger to press the button
            //drawTool.SkipNextDraw();
        }
    }
}

