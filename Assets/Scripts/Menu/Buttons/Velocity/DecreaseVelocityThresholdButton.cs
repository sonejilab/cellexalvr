using CellexalVR.AnalysisObjects;
using TMPro;

namespace CellexalVR.Menu.Buttons.Velocity
{
    public class DecreaseVelocityThresholdButton : CellexalButton
    {

        public float amount = 0.5f;

        protected override string Description
        {
            get
            {
                return "Decrease threshold, show more velocities";
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
