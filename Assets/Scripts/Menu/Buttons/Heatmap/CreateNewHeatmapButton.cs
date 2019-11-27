using CellexalVR.Interaction;

namespace CellexalVR.Menu.Buttons.Heatmap
{
    /// <summary>
    /// Represents the button used for creating a new heatmap from a selection on the heatmap.
    /// </summary>
    class CreateNewHeatmapButton : CellexalButton
    {
        private CellexalVR.AnalysisObjects.Heatmap heatmap;
        private HeatmapRaycast heatmapRaycast;

        protected override string Description
        {
            get { return "Create New Heatmap From Selection"; }
        }

        private void Start()
        {
            heatmapRaycast = GetComponentInParent<HeatmapRaycast>();
            heatmap = GetComponentInParent<AnalysisObjects.Heatmap>();

        }

        public override void Click()
        {
            heatmapRaycast.CreateNewHeatmapFromSelection();
            device.TriggerHapticPulse(2000);
        }
    }
}