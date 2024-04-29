using CellexalVR.Spatial;

namespace CellexalVR.Menu.Buttons
{
    public class DestroySpatialPartButton : CellexalButton
    {

        private string modelName;

        private void Start()
        {
            modelName = GetComponentInParent<BrainPartButton>().ModelName;
        }


        protected override string Description => $"Destroy {modelName} mesh from reference";

        public override void Click()
        {
            GetComponentInParent<AllenReferenceBrain>().RemovePart(modelName);
        }

    }
}
