using UnityEngine;

///<summary>
/// Represents a button used the graphs from the cell selection used for this particular heatmap.
///</summary>
public class RecolourHeatmapButton : CellexalButton
{
    protected override string Description
    {
        get
        {
            return "";
        }
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Click()
    {
        gameObject.GetComponentInParent<Heatmap>().ColorCells();
    }
}
