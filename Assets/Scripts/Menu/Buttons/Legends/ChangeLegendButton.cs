using CellexalVR.AnalysisObjects;
using CellexalVR.Extensions;

namespace CellexalVR.Menu.Buttons.Legends
{

    /// <summary>
    /// Represents the tab buttons on the histogram that switch between the 10 last colored genes.
    /// </summary>
    public class ChangeLegendButton : EnvironmentTabButton
    {
        public AnalysisObjects.LegendManager.Legend legendToActivate;

        protected override string Description => "Switch to " + legendToActivate.ToDescriptionString();

        public override void Click()
        {
            referenceManager.legendManager.ActivateLegend(legendToActivate);
            referenceManager.multiuserMessageSender.SendMessageChangeLegend(legendToActivate.ToString());
        }
    }
}
