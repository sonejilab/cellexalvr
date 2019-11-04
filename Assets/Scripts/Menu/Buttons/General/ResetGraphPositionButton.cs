using CellexalVR.AnalysisObjects;
using CellexalVR.General;

namespace CellexalVR.Menu.Buttons.General
{
    ///<summary>
    /// Represents a button used for resetting the color and position of the graphs.
    ///</summary>
    public class ResetGraphPositionButton : CellexalButton
    {

        private GraphManager graphManager;

        protected override string Description
        {
            get
            {
                return "Reset Position Of All Graphs";
            }
        }

        private void Start()
        {
            graphManager = referenceManager.graphManager;
            SetButtonActivated(false);
            CellexalEvents.GraphsLoaded.AddListener(OnGraphsLoaded);
            CellexalEvents.GraphsUnloaded.AddListener(OnGraphsUnloaded);
        }

        public override void Click()
        {
            graphManager.ResetGraphsPosition();
            referenceManager.multiuserMessageSender.SendMessageResetGraphPosition();
        }

        private void OnGraphsLoaded()
        {
            SetButtonActivated(true);
        }

        private void OnGraphsUnloaded()
        {
            SetButtonActivated(false);
        }
    }
}