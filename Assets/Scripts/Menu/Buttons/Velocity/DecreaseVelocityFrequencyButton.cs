using CellexalVR.AnalysisObjects;
using TMPro;

namespace CellexalVR.Menu.Buttons.Velocity
{
    public class DecreaseVelocityFrequencyButton : CellexalButton
    {
        public float amount = -1f;
        public TextMeshPro frequencyText;

        protected override string Description
        {
            get
            {
                return "Decrease velocity emitting frequency";
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
                frequencyText.text = "Frequency: " + newFrequencyString;
            }
        }
    }
}