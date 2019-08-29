using CellexalVR.AnalysisObjects;
using TMPro;

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
            Graph activeGraph = referenceManager.velocityGenerator.ActiveGraph;
            if (activeGraph != null)
            {
                float newFrequency = activeGraph.velocityParticleEmitter.ChangeFrequency(amount);
                string newFrequencyString = (1f / newFrequency).ToString();
                if (newFrequencyString.Length > 4)
                {
                    newFrequencyString = newFrequencyString.Substring(0, 4);
                }
                referenceManager.velocitySubMenu.frequencyText.text = "Frequency: " + newFrequencyString;
                referenceManager.gameManager.InformChangeFrequency(activeGraph.GraphName, amount);
            }
        }
    }
}
