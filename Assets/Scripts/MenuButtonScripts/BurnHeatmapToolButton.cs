///<summary>
/// Represents a button used for toggling the delete tool. Delete tool does not delete graphs.
///</summary>
public class BurnHeatmapToolButton : CellexalToolButton
{
    protected override string Description
    {
        get
        {
            return "Delete tool";
        }
    }

    protected override ControllerModelSwitcher.Model ControllerModel
    {
        get { return ControllerModelSwitcher.Model.DeleteTool; }
    }


}
