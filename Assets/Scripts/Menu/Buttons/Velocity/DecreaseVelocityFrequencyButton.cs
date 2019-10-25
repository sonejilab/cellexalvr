using CellexalVR.AnalysisObjects;
using System.Collections.Generic;

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
            List<Graph> activeGraphs = referenceManager.velocityGenerator.ActiveGraphs;
            foreach (Graph g in activeGraphs)
            {
                g.velocityParticleEmitter.ChangeFrequency(amount);
                referenceManager.gameManager.InformChangeFrequency(amount);
            }
        }
    }
}
