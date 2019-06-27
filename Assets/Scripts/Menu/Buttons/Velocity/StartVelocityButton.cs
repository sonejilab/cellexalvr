using CellexalVR.AnalysisObjects;

namespace CellexalVR.Menu.Buttons.Velocity
{
    public class StartVelocityButton : CellexalButton
    {
        protected override string Description
        {
            get
            {
                return "Start velocity";
            }
        }

        public override void Click()
        {
            Graph activeGraph = referenceManager.velocityGenerator.ActiveGraph;
            if (activeGraph != null)
            {
                activeGraph.velocityParticleEmitter.Play();
            }
        }
    }
}
