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
            referenceManager.velocityGenerator.ChangeConstantSynchedMode();
            referenceManager.multiuserMessageSender.SendMessageConstantSynchedMode();
        }
    }
}
