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
        emitter.ChangeSpeed(amount);
        referenceManager.gameManager.InformChangeSpeed(activeGraph.GraphName, amount);
    }
}
