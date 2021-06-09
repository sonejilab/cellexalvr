using CellexalVR.AnalysisObjects;
using CellexalVR.General;

namespace CellexalVR.Menu.Buttons.General
{
    /// <summary>
    /// Toggles graph axes labels on/off.
    /// </summary>
    public class ToggleAxesButton : CellexalButton
    {
        private GraphManager graphManager;

        void Start()
        {
            graphManager = referenceManager.graphManager;
            //GetComponent<SimpleTextRotator>().SetTransforms(this.transform, this.transform);
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
            graphManager.ToggleAxes();
            referenceManager.multiuserMessageSender.SendMessageToggleAxes();
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