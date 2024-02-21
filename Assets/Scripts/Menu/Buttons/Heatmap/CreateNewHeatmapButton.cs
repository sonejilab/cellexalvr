using CellexalVR.General;
using CellexalVR.Interaction;

namespace CellexalVR.Menu.Buttons.Heatmap
{
    /// <summary>
    /// Represents the button used for creating a new heatmap from a selection on the heatmap.
    /// </summary>
    class CreateNewHeatmapButton : CellexalButton
    {
        private HeatmapRaycast heatmapRaycast;

        protected override string Description
        {
            get { return "Create New Heatmap From Selection"; }
        }

        private void Start()
        {
            heatmapRaycast = GetComponentInParent<HeatmapRaycast>();

        }

        public override void Click()
        {
            heatmapRaycast.CreateNewHeatmapFromSelection();
            ReferenceManager.instance.rightController.SendHapticImpulse(0.8f, 0.3f);
        }
    }
}