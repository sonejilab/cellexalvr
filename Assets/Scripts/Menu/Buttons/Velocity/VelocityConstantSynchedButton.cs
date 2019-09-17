using CellexalVR.AnalysisObjects;
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
            Graph activeGraph = referenceManager.velocityGenerator.ActiveGraph;
            if (activeGraph != null)
            {
                bool switchToConstant = !activeGraph.velocityParticleEmitter.ConstantEmitOverTime;
                activeGraph.velocityParticleEmitter.ConstantEmitOverTime = switchToConstant;
                referenceManager.gameManager.InformConstantSynchedMode(activeGraph.GraphName, switchToConstant);
            }
        }
    }
}
