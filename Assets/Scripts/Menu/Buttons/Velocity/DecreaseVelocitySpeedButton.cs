using CellexalVR.AnalysisObjects;
using CellexalVR.Menu.Buttons;
using TMPro;

public class DecreaseVelocitySpeedButton : CellexalButton
{

    public float amount;

    protected override string Description
    {
        get
        {
            return "Decrease speed of velocity arrows";
        }
    }

    public override void Click()
    {
        Graph activeGraph = referenceManager.velocityGenerator.ActiveGraph;
        VelocityParticleEmitter emitter = activeGraph.velocityParticleEmitter;
        float newSpeed = emitter.ChangeSpeed(amount);
        referenceManager.velocitySubMenu.speedText.text = "Speed: " + newSpeed;
        referenceManager.gameManager.InformChangeSpeed(activeGraph.GraphName, amount);
    }
}
