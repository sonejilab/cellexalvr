using UnityEngine;
using System.Collections;
using CellexalVR.AnalysisObjects;

namespace CellexalVR.Menu.Buttons
{

    public class ChangeVelocityParticleButton : CellexalButton
    {

        protected override string Description
        {
            get
            {
                return "Change between arrow or circle particle";
            }
        }


        public override void Click()
        {
            Graph activeGraph = referenceManager.velocityGenerator.ActiveGraph;
            if (activeGraph != null)
            {
                bool switchToArrow = !activeGraph.velocityParticleEmitter.UseArrowParticle;
                activeGraph.velocityParticleEmitter.UseArrowParticle = switchToArrow;
                referenceManager.gameManager.InformChangeParticleMode(activeGraph.GraphName, switchToArrow);
            }
        }
    }
}
