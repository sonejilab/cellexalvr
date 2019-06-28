using CellexalVR.AnalysisObjects;
using TMPro;

namespace CellexalVR.Menu.Buttons.Velocity
{
    public class IncreaseVelocityThresholdButton : CellexalButton
    {

        public float amount = 2f;
        public TextMeshPro thresholdText;

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
                float newThreshold = activeGraph.velocityParticleEmitter.ChangeThreshold(amount);
                thresholdText.text = "Threshold: " + newThreshold;
            }
        }
    }
}
