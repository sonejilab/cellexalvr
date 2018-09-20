
///<summary>
/// Represents a button used for toggling the selection tool.
///</summary>
public class SelectionToolButton : CellexalToolButton
{
    protected override string Description
    {
        get { return "Toggle selection tool"; }
    }
    protected override ControllerModelSwitcher.Model ControllerModel
    {
        get { return ControllerModelSwitcher.Model.SelectionTool; }
    }
}
