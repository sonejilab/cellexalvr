using CellexalVR.AnalysisObjects;
using CellexalVR.General;

namespace CellexalVR.Menu.Buttons
{
    public class ToggleTransparencyButton : CellexalButton
    {
        private GraphManager graphManager;
        private bool toggle;

        protected override string Description
        {
            get
            {
                return "Toggle transparency of all graph points";
            }
        }

        public override void Click()
        {
            referenceManager.multiuserMessageSender.SendMessageClearExpressionColours();
            graphManager.ToggleGraphPointTransparency(!toggle);
            toggle = !toggle;
        }

        // Use this for initialization
        void Start()
        {
            graphManager = referenceManager.graphManager;
        }

    }

}
