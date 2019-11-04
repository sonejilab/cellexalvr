using CellexalVR.AnalysisObjects;
using System.Collections.Generic;

namespace CellexalVR.Menu.Buttons.Velocity
{
    public class StopVelocityButton : CellexalButton
    {
        protected override string Description
        {
            get
            {
                return "Stop velocity";
            }
        }

        public override void Click()
        {
            List<Graph> activeGraphs = referenceManager.velocityGenerator.ActiveGraphs;
            foreach (Graph g in activeGraphs)
            {
                g.velocityParticleEmitter.Stop();
            }
            referenceManager.multiuserMessageSender.SendMessageStopVelocity();
        }
    }
}
