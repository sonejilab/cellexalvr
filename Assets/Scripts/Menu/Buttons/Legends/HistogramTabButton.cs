using CellexalVR.AnalysisObjects;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.Legends
{

    /// <summary>
    /// Represents the tab buttons on the histogram that switch between the 10 last colored genes.
    /// </summary>
    public class HistogramTabButton : EnvironmentTabButton
    {
        public int index;
        [HideInInspector]
        public string geneName;

        protected override string Description => "Switch to " + geneName;

        public override void Click()
        {
            referenceManager.legendManager.geneExpressionHistogram.SwitchToTab(index);
            referenceManager.multiuserMessageSender.SendMessageChangeTab(index);
        }
    }
}
