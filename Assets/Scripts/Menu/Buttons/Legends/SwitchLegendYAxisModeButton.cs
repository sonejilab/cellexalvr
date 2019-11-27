using UnityEngine;
using System.Collections;
using CellexalVR.AnalysisObjects;

namespace CellexalVR.Menu.Buttons.Legends
{

    public class SwitchLegendYAxisModeButton : CellexalButton
    {
        protected override string Description => "Switch Y axis mode";

        private GeneExpressionHistogram.YAxisMode currentMode;

        public override void Click()
        {
            if (currentMode == GeneExpressionHistogram.YAxisMode.Linear)
            {
                currentMode = GeneExpressionHistogram.YAxisMode.Logarithmic;
            }
            else if (currentMode == GeneExpressionHistogram.YAxisMode.Logarithmic)
            {
                currentMode = GeneExpressionHistogram.YAxisMode.Linear;
            }

            gameObject.GetComponentInParent<GeneExpressionHistogram>().RecreateHistogram(currentMode);
        }
    }
}
