using CellexalVR.AnalysisObjects;
using CellexalVR.Menu.Buttons;
using System.Collections.Generic;

public class IncreaseVelocitySpeedButton : CellexalButton
{
    public float amount;

    protected override string Description
    {
        get
        {
            return "Increase speed of velocity arrows";
        }
    }

    public override void Click()
    {
        List<Graph> activeGraphs = referenceManager.velocityGenerator.ActiveGraphs;
        foreach (Graph g in activeGraphs)
        {
            g.velocityParticleEmitter.ChangeSpeed(amount);
            referenceManager.gameManager.InformChangeSpeed(amount);
        }
    }
}
