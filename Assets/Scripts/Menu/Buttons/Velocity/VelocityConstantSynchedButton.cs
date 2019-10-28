using CellexalVR.AnalysisObjects;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CellexalVR.Menu.Buttons
{

    public class VelocityConstantSynchedButton : CellexalButton
    {

        protected override string Description
        {
            get
            {
                return "Change between constant or synched mode";
            }
        }


        public override void Click()
        {
            List<Graph> activeGraphs = referenceManager.velocityGenerator.ActiveGraphs;
            bool switchToConstant = false;
            foreach (Graph g in activeGraphs)
            {
                switchToConstant = !g.velocityParticleEmitter.ConstantEmitOverTime;
                g.velocityParticleEmitter.ConstantEmitOverTime = switchToConstant;
            }
            referenceManager.gameManager.InformConstantSynchedMode(switchToConstant);
        }
    }
}
