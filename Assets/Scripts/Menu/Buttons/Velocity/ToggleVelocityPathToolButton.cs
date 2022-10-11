using CellexalVR.AnalysisObjects;
using System.Collections.Generic;

namespace CellexalVR.Menu.Buttons.Velocity
{

    public class ToggleVelocityPathToolButton : CellexalButton
    {

        protected override string Description
        {
            get
            {
                return "Toggle graphpoints";
            }
        }

        public override void Click()
        {
            referenceManager.velocityPathTool.ToggleActive();
        }
    }
}