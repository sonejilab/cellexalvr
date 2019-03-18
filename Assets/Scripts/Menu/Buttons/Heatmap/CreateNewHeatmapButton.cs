using CellexalVR.AnalysisObjects;

namespace CellexalVR.Menu.Buttons.Heatmap
{
    /// <summary>
    /// Represents the button used for creating a new heatmap from a selection on the heatmap.
    /// </summary>
    class CreateNewHeatmapButton : CellexalButton
    {
        protected override string Description
        {
            get { return "Create a new heatmap from your selection"; }
        }

        public override void Click()
        {
            var heatmap = GetComponentInParent<AnalysisObjects.Heatmap>();
            referenceManager.gameManager.InformCreateNewHeatmapFromSelection(heatmap.name);
            heatmap.CreateNewHeatmapFromSelection();
            device.TriggerHapticPulse(2000);
        }
    }
}