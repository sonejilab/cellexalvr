
namespace CellexalVR.Menu.Buttons.Legends
{

    /// <summary>
    /// Represents the tab buttons on the histogram that switch between the 10 last colored genes.
    /// </summary>
    public class ChangeLegendButton : CellexalButton
    {
        public AnalysisObjects.LegendManager.Legend legendToActivate;

        protected override string Description => "Switch legend to " + legendToActivate.ToString();

        public override void Click()
        {
            referenceManager.legendManager.ActivateLegend(legendToActivate);
        }
    }
}
