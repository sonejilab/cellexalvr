﻿using CellexalVR.AnalysisObjects;
using System.Collections.Generic;

namespace CellexalVR.Menu.Buttons.Velocity
{
    public class IncreaseVelocityFrequencyButton : CellexalButton
    {
        /// <summary>
        /// How much the delay between the emits is changed in seconds. Negative numbers mean more often, positive numbers mean less often.
        /// </summary>
        public float amount = 1f;

        protected override string Description
        {
            get
            {
                return "Increase velocity emitting frequency";
            }
        }

        public override void Click()
        {
            referenceManager.velocityGenerator.ChangeFrequency(amount);
            referenceManager.multiuserMessageSender.SendMessageChangeFrequency(amount);
        }
    }
}
