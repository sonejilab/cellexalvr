using CellexalVR.General;
namespace CellexalVR.Menu.Buttons.Selection
{
    ///<summary>
    /// Represents a button used for undoing the current selection.
    ///</summary>
    public class UndoSelectionButton : CellexalButton
    {
        //private SelectionToolHandler selectionToolHandler;
        private SelectionManager selectionManager;

        protected override string Description
        {
            get { return "Cancel selection"; }
        }

        protected void Start()
        {

            selectionManager = referenceManager.selectionManager;
            SetButtonActivated(false);
            CellexalEvents.SelectionStarted.AddListener(TurnOn);
            CellexalEvents.SelectionCanceled.AddListener(TurnOff);
            CellexalEvents.SelectionConfirmed.AddListener(TurnOff);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
        }

        public override void Click()
        {
            referenceManager.gameManager.InformCancelSelection();
            selectionManager.CancelSelection();
            SetButtonActivated(false);
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