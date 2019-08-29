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
                float newThreshold = activeGraph.velocityParticleEmitter.ChangeThreshold(amount);
                referenceManager.velocitySubMenu.thresholdText.text = "Threshold: " + newThreshold;
                referenceManager.gameManager.InformChangeThreshold(activeGraph.GraphName, amount);
            }
        }
    }
}
