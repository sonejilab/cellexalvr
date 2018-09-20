/// <summary>
/// Represents the button that toggles the draw tool.
/// </summary>
public class DrawToolButton : CellexalToolButton
{
    protected override string Description
    {
        get { return "Toggles the draw tool"; }
    }

    protected override ControllerModelSwitcher.Model ControllerModel
    {
        get { return ControllerModelSwitcher.Model.DrawTool; }
    }
}

