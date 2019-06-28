using CellexalVR.AnalysisObjects;
using CellexalVR.Menu.Buttons;
using TMPro;

public class IncreaseVelocitySpeedButton : CellexalButton
{
    public float amount;
    public TextMeshPro speedText;

    protected override string Description
    {
        get
        {
            return "Increase speed of velocity arrows";
        }
    }

    public override void Click()
    {
        VelocityParticleEmitter emitter = referenceManager.velocityGenerator.ActiveGraph.velocityParticleEmitter;
        float newSpeed = emitter.ChangeSpeed(amount);
        speedText.text = "Speed: " + newSpeed;
    }

}
