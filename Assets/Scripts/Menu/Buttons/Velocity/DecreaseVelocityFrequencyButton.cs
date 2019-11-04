using CellexalVR.AnalysisObjects;
using System.Collections.Generic;

namespace CellexalVR.Menu.Buttons.Velocity
{
    public class DecreaseVelocityFrequencyButton : CellexalButton
    {
        public float amount = -1f;

        protected override string Description
        {
            get
            {
                return "Decrease velocity emitting frequency";
            }
        }

        public override void Click()
        {
            referenceManager.velocityGenerator.ChangeFrequency(amount);
            referenceManager.multiuserMessageSender.SendMessageChangeFrequency(amount);
        }
    }
}
