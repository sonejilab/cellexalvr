using CellexalVR.AnalysisObjects;
using System.Collections.Generic;

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
            List<Graph> activeGraphs = referenceManager.velocityGenerator.ActiveGraphs;
            foreach (Graph g in activeGraphs)
            {
                g.velocityParticleEmitter.ChangeThreshold(amount);
                referenceManager.gameManager.InformChangeThreshold(amount);
            }
        }
    }
}
