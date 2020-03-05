using CellexalVR.AnalysisObjects;

namespace CellexalVR.Menu.Buttons.Legends
{

    public class SwitchLegendYAxisModeButton : CellexalButton
    {
        public GeneExpressionHistogram histogram;

        protected override string Description => "Switch Y axis mode";

        public override void Click()
        {
            if (histogram.DesiredYAxisMode == GeneExpressionHistogram.YAxisMode.Linear)
            {
                histogram.DesiredYAxisMode = GeneExpressionHistogram.YAxisMode.Logarithmic;
            }
            else if (histogram.DesiredYAxisMode == GeneExpressionHistogram.YAxisMode.Logarithmic)
            {
                histogram.DesiredYAxisMode = GeneExpressionHistogram.YAxisMode.Linear;
            }
            histogram.RecreateHistogram();
            referenceManager.multiuserMessageSender.SendMessageSwitchMode(histogram.DesiredYAxisMode.ToString());
        }
    }
}
