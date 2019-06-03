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
        private bool activate;

        void Start()
        {
            graphManager = referenceManager.graphManager;
            //GetComponent<SimpleTextRotator>().SetTransforms(this.transform, this.transform);
            activate = false;
            CellexalEvents.GraphsLoaded.AddListener(TurnOn);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
        }

        protected override string Description
        {
            get
            {
                return "Show Axes of object";
            }
        }

        public override void Click()
        {
            graphManager.SetAxesVisible(activate);
            activate = !activate;
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