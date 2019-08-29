using CellexalVR.AnalysisObjects;
using TMPro;
using UnityEngine;

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
            Graph activeGraph = referenceManager.velocityGenerator.ActiveGraph;
            if (activeGraph != null)
            {
                bool switchToGraphpointColors = !activeGraph.velocityParticleEmitter.UseGraphPointColors;
                activeGraph.velocityParticleEmitter.UseGraphPointColors = switchToGraphpointColors;
                if (switchToGraphpointColors)
                {
                    referenceManager.velocitySubMenu.graphPointColorsModeText.text = "Mode: Graphpoint colors";
                }
                else
                {
                    referenceManager.velocitySubMenu.graphPointColorsModeText.text = "Mode: Gradient";
                }
                referenceManager.gameManager.InformGraphPointColorsMode(activeGraph.GraphName, switchToGraphpointColors);
            }
        }
    }
}
