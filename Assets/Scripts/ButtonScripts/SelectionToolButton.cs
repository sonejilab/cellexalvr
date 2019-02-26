
///<summary>
/// Represents a button used for toggling the selection tool.
///</summary>
public class SelectionToolButton : CellexalToolButton
{
    protected override string Description
    {
        get { return "Toggle selection tool"; }
    }

    protected override void Awake()
    {
        base.Awake();
        //CellexalEvents.CreatingNetworks.AddListener(TurnOff);
        CellexalEvents.NetworkCreated.AddListener(TurnOn);
        //CellexalEvents.CreatingHeatmap.AddListener(TurnOff);
        //CellexalEvents.HeatmapCreated.AddListener(TurnOn);
    }

    protected override ControllerModelSwitcher.Model ControllerModel
    {
        get { return ControllerModelSwitcher.Model.SelectionTool; }
    }
    
}
