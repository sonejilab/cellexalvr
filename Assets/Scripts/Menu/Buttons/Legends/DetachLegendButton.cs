using CellexalVR.AnalysisObjects;

namespace CellexalVR.Menu.Buttons.Legends
{
    public class DetachLegendButton : CellexalButton
    {
        protected override string Description => "Detach legend";

        public override void Click()
        {
            LegendManager legendManager = referenceManager.legendManager;
            legendManager.DetachLegendFromCube();
        }
    }
}
