using CellexalVR.AnalysisObjects;
using TMPro;

namespace CellexalVR.Menu.Buttons.Velocity
{
    public class DecreaseVelocityThresholdButton : CellexalButton
    {

        public float amount = 0.5f;
        public TextMeshPro thresholdText;

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
                thresholdText.text = "Threshold: " + newThreshold;
            }
        }
    }
}
