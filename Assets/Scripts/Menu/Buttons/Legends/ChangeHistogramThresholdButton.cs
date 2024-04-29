namespace CellexalVR.Menu.Buttons.Legends
{

    public class ChangeHistogramThresholdButton : CellexalVR.Menu.Buttons.CellexalButton
    {

        public bool increment;
        protected override string Description => (increment ? "Increase" : "Decrease") + " number of tallest bars to skip when scaling y axis";

        public override void Click()
        {
            var histogram = gameObject.GetComponentInParent<CellexalVR.AnalysisObjects.GeneExpressionHistogram>();
            int i = increment ? 1 : -1;
            histogram.TallestBarsToSkip += i;
            histogram.RecreateHistogram();
            referenceManager.multiuserMessageSender.SendMessageChangeThreshold(i);
        }
    }

}
