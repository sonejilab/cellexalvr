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
            List<Graph> activeGraphs = referenceManager.velocityGenerator.ActiveGraphs;
            foreach (Graph g in activeGraphs)
            {
                g.ToggleGraphPoints();
            }
            referenceManager.gameManager.InformToggleGraphPoints();
        }
    }
}