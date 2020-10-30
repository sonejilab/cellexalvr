using CellexalVR.AnalysisObjects;
using CellexalVR.General;

namespace CellexalVR.Menu.Buttons.General
{
    /// <summary>
    /// Toggles graph information labels on/off.
    /// </summary>
    public class ShowLabelsButton : CellexalButton
    {
        private GraphManager graphManager;
        private bool activate;

        private void Start()
        {
            graphManager = referenceManager.graphManager;
            CellexalEvents.GraphsLoaded.AddListener(TurnOn);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
        }

        protected override string Description => "Show Labels Of Objects";

        public override void Click()
        {
            graphManager.ToggleInfoPanels();
        }

        private void TurnOn()
        {
            SetButtonActivated(true);
        }

        private void TurnOff()
        {
            SetButtonActivated(false);
        }
    }
}