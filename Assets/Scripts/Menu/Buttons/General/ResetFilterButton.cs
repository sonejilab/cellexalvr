namespace CellexalVR.Menu.Buttons
{

    public class ResetFilterButton : CellexalVR.Menu.Buttons.CellexalButton
    {
        protected override string Description => "Resets the current filter";

        public override void Click()
        {
            referenceManager.filterManager.ResetFilter();
        }
    }
}
