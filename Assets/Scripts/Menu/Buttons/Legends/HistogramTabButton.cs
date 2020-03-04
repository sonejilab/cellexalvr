
namespace CellexalVR.Menu.Buttons.Legends
{

    /// <summary>
    /// Represents the tab buttons on the histogram that switch between the 10 last colored genes.
    /// </summary>
    public class HistogramTabButton : CellexalButton
    {
        public int index;

        protected override string Description => "Switch tab to " + index;

        public override void Click()
        {
            referenceManager.legendManager.geneExpressionHistogram.SwitchToTab(index);
            referenceManager.multiuserMessageSender.SendMessageChangeTab(index);
        }
    }
}
