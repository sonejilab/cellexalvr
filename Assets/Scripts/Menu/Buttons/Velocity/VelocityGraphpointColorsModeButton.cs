using CellexalVR.AnalysisObjects;
using System.Collections.Generic;

namespace CellexalVR.Menu.Buttons
{

    public class VelocityGraphpointColorsModeButton : CellexalButton
    {

        protected override string Description
        {
            get
            {
                return "Change between gradient or graphpoint colors";
            }
        }

        public override void Click()
        {
            List<Graph> activeGraphs = referenceManager.velocityGenerator.ActiveGraphs;
            bool switchToGraphpointColors = false;
            foreach (Graph g in activeGraphs)
            {
                switchToGraphpointColors = !g.velocityParticleEmitter.UseGraphPointColors;
                g.velocityParticleEmitter.UseGraphPointColors = switchToGraphpointColors;
            }
            referenceManager.gameManager.InformGraphPointColorsMode(switchToGraphpointColors);
        }
    }
}
