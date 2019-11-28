public class ChangeHistogramThresholdButton : CellexalVR.Menu.Buttons.CellexalButton
{

    public bool increment;
    protected override string Description => "Change number of tallest bars to skip when scaling y axis";

    public override void Click()
    {
        var histogram = gameObject.GetComponentInParent<CellexalVR.AnalysisObjects.GeneExpressionHistogram>();
        histogram.TallestBarsToSkip += increment ? 1 : -1;
        histogram.RecreateHistogram();
    }
}
