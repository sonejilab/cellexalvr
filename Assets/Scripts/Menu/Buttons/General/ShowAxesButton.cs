using CellexalVR.AnalysisObjects;
using CellexalVR.General;

namespace CellexalVR.Menu.Buttons.General
{
    /// <summary>
    /// Toggles graph axes labels on/off.
    /// </summary>
    public class ShowAxesButton : CellexalButton
    {
        private GraphManager graphManager;
        private bool activated;

        void Start()
        {
            graphManager = referenceManager.graphManager;
            //GetComponent<SimpleTextRotator>().SetTransforms(this.transform, this.transform);
            activated = false;
            CellexalEvents.GraphsLoaded.AddListener(TurnOn);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
        }

        protected override string Description
        {
            get
            {
                return "Show Axes Of Objects";
            }
        }

        public override void Click()
        {
            activated = !activated;
            graphManager.ToggleAxes();
        }

        void TurnOn()
        {
            SetButtonActivated(true);
        }

        void TurnOff()
        {
            SetButtonActivated(false);
        }
    }
}