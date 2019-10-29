using CellexalVR.AnalysisObjects;
using System.Collections.Generic;

namespace CellexalVR.Menu.Buttons
{

    public class ToggleGraphpointsButton : CellexalButton
    {

        protected override string Description
        {
            get
            {
                return "Toggle graphpoints";
            }
        }

        public override void Click()
        {
            referenceManager.velocityGenerator.ToggleGraphPoints();
            referenceManager.gameManager.InformToggleGraphPoints();
        }
    }
}