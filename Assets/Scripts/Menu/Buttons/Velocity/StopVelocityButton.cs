using CellexalVR.AnalysisObjects;

namespace CellexalVR.Menu.Buttons.Velocity
{
    public class StopVelocityButton : CellexalButton
    {
        protected override string Description
        {
            get
            {
                return "Stop velocity";
            }
        }

        public override void Click()
        {
            Graph activeGraph = referenceManager.velocityGenerator.ActiveGraph;
            if (activeGraph != null)
            {
                activeGraph.velocityParticleEmitter.Stop();
                referenceManager.gameManager.InformStopVelocity(activeGraph.GraphName);
                referenceManager.velocitySubMenu.DeactivateOutlines();
            }
        }
    }
}
