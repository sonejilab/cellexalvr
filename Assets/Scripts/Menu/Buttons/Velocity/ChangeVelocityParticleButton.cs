using UnityEngine;
using System.Collections.Generic;
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
            List<Graph> activeGraphs = referenceManager.velocityGenerator.ActiveGraphs;
            if (activeGraphs.Count > 0)
            {
                bool switchToArrow = false;
                foreach (Graph g in activeGraphs)
                {
                    switchToArrow = !g.velocityParticleEmitter.UseArrowParticle;
                    g.velocityParticleEmitter.UseArrowParticle = switchToArrow;
                }
                referenceManager.gameManager.InformChangeParticleMode(switchToArrow);
            }
        }
    }
}
