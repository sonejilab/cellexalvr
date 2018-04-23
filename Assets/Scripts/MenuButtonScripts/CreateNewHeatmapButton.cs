/// <summary>
/// Represents the button used for creating a new heatmap from a selection on the heatmap.
/// </summary>
class CreateNewHeatmapButton : CellexalButton
{
    protected override string Description
    {
        get { return "Create a new heatmap from your selection"; }
    }

    protected override void Click()
    {
        GetComponentInParent<Heatmap>().CreateNewHeatmapFromSelection();
    }
}
