namespace CellexalVR.Menu.Buttons.Heatmap
{
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

        public override void Click()
        {
            gameObject.GetComponentInParent<AnalysisObjects.Heatmap>().ColorCells();
        }
    }
}