using CellexalVR.AnalysisObjects;
using TMPro;
using UnityEngine;

namespace CellexalVR.Menu.Buttons
{

    public class VelocityGraphpointColorsModeButton : CellexalButton
    {

        public TextMeshPro graphpointColorsModeText;

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
                bool switchToGraphpointCOlors = !activeGraph.velocityParticleEmitter.UseGraphPointColors;
                activeGraph.velocityParticleEmitter.UseGraphPointColors = switchToGraphpointCOlors;
                if (switchToGraphpointCOlors)
                {
                    graphpointColorsModeText.text = "Mode: Graphpoint colors";
                }
                else
                {
                    graphpointColorsModeText.text = "Mode: Gradient";
                }
            }
        }
    }
}
