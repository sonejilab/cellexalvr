using CellexalVR.AnalysisObjects;
using System.Collections.Generic;

namespace CellexalVR.Menu.Buttons.Velocity
{
    public class IncreaseVelocityThresholdButton : CellexalButton
    {

        public float amount = 2f;

        protected override string Description
        {
            get
            {
                return "Increase threshold, show less velocities";
            }
        }

        public override void Click()
        {
            referenceManager.velocityGenerator.ChangeThreshold(amount);
            referenceManager.multiuserMessageSender.SendMessageChangeThreshold(amount);
        }
    }
}
