using CellexalVR.AnalysisObjects;

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
            Graph activeGraph = referenceManager.velocityGenerator.ActiveGraph;
            if (activeGraph != null)
            {
                activeGraph.ToggleGraphPoints();
            }
        }
    }
}