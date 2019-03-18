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
                return "Show labels of object";
            }
        }

        public override void Click()
        {
            graphManager.SetInfoPanelsVisible(activate);
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