namespace CellexalVR.Menu.Buttons
{

    public class ResetFilterButton : CellexalVR.Menu.Buttons.CellexalButton
    {
        protected override string Description => "Resets the current filter";

        private void Start()
        {
            CellexalVR.General.CellexalEvents.FilterActivated.AddListener(OnFilterActivated);
            CellexalVR.General.CellexalEvents.FilterDeactivated.AddListener(OnFilterDeactivated);
            SetButtonActivated(false);
        }

        public override void Click()
        {
            referenceManager.filterManager.ResetFilter();
            referenceManager.multiuserMessageSender.SendMessageResetFilter();
        }


        private void OnFilterActivated()
        {
            SetButtonActivated(true);
        }
        private void OnFilterDeactivated()
        {
            SetButtonActivated(false);
        }
    }
}
