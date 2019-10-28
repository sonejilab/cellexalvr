using CellexalVR.AnalysisObjects;
using System.Collections.Generic;

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
            List<Graph> activeGraphs = referenceManager.velocityGenerator.ActiveGraphs;
            foreach (Graph g in activeGraphs)
            {
                g.velocityParticleEmitter.ChangeThreshold(amount);
                referenceManager.gameManager.InformChangeThreshold(amount);
            }
        }
    }
}
