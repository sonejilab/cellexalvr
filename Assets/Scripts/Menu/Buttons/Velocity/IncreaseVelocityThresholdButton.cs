using CellexalVR.AnalysisObjects;
using TMPro;

namespace CellexalVR.Menu.Buttons.Velocity
{
    public class IncreaseVelocityThresholdButton : CellexalButton
    {

        public float amount = 2f;

        protected override string Description
        {
            get
            {
                return "Increase threshold, show less velocities";
            }
        }

        public override void Click()
        {
            Graph activeGraph = referenceManager.velocityGenerator.ActiveGraph;
            if (activeGraph != null)
            {
                activeGraph.velocityParticleEmitter.ChangeThreshold(amount);
                referenceManager.gameManager.InformChangeThreshold(activeGraph.GraphName, amount);
            }
        }
    }
}
