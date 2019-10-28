using CellexalVR.AnalysisObjects;
using System.Collections.Generic;

namespace CellexalVR.Menu.Buttons.Velocity
{
    public class StartVelocityButton : CellexalButton
    {
        protected override string Description
        {
            get
            {
                return "Start velocity";
            }
        }

        public override void Click()
        {
            List<Graph> activeGraphs = referenceManager.velocityGenerator.ActiveGraphs;
            foreach (Graph g in activeGraphs)
            {
                g.velocityParticleEmitter.Play();
            }
            referenceManager.gameManager.InformStartVelocity();
        }
    }
}
