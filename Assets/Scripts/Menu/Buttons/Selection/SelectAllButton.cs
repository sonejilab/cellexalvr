using CellexalVR.General;
namespace CellexalVR.Menu.Buttons.Selection
{
    ///<summary>
    /// Represents a button used for undoing the current selection.
    ///</summary>
    public class SelectAllButton : CellexalButton
    {
        //private SelectionToolHandler selectionToolHandler;
        private SelectionManager selectionManager;

        protected override string Description
        {
            get { return "Select All as Current Group"; }
        }

        protected void Start()
        {

            selectionManager = referenceManager.selectionManager;
            SetButtonActivated(false);
            CellexalEvents.GraphsLoaded.AddListener(TurnOn);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
        }

        public override void Click()
        {
            selectionManager.SelectAll();
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
