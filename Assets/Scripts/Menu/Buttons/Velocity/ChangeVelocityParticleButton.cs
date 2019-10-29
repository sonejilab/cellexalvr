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
            referenceManager.velocityGenerator.ChangeParticle();
            referenceManager.gameManager.InformChangeParticleMode();
        }
    }
}
