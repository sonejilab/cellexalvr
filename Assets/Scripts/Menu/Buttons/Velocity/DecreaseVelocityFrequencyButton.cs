using CellexalVR.AnalysisObjects;
using TMPro;

namespace CellexalVR.Menu.Buttons.Velocity
{
    public class DecreaseVelocityFrequencyButton : CellexalButton
    {
        public float amount = -1f;

        protected override string Description
        {
            get
            {
                return "Decrease velocity emitting frequency";
            }
        }

        public override void Click()
        {
            Graph activeGraph = referenceManager.velocityGenerator.ActiveGraph;
            if (activeGraph != null)
            {
                activeGraph.velocityParticleEmitter.ChangeFrequency(amount);
                referenceManager.gameManager.InformChangeFrequency(activeGraph.GraphName, amount);
            }
        }
    }
}
