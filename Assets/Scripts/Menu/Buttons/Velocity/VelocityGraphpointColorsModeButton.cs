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
            referenceManager.velocityGenerator.ChangeGraphPointColorMode();
            referenceManager.multiuserMessageSender.SendMessageGraphPointColorsMode();
        }
    }
}
