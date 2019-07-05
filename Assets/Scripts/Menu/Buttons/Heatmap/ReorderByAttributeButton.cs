namespace CellexalVR.Menu.Buttons.Heatmap
{
    ///<summary>
    /// Represents a button used the graphs from the cell selection used for this particular heatmap.
    ///</summary>
    public class ReorderByAttributeButton : CellexalButton
    {
        private bool toggle;
        private CellexalVR.AnalysisObjects.Heatmap heatmap;

        protected override string Description
        {
            get
            {
                if (!toggle)
                {
                    return "Reorder heatmap so attribute bar is sorted in each group";
                }
                else
                {
                    return "Switch back to original ordering";
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            heatmap = gameObject.GetComponentInParent<AnalysisObjects.Heatmap>();
        }


        public override void Click()
        {
            if (!toggle)
            {
                heatmap.ReorderByAttribute();
            }
            else
            {
                heatmap.BuildTexture(heatmap.selection, "");
            }
            toggle = !toggle;
        }
    }
}