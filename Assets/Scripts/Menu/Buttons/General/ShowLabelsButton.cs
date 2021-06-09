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
                return "Show Labels Of Objects";
            }
        }

        public override void Click()
        {
            graphManager.ToggleInfoPanels();
            referenceManager.multiuserMessageSender.SendMessageToggleInfoPanels();
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