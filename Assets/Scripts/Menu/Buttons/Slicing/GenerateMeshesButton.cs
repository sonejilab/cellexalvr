using CellexalVR.Spatial;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class GenerateMeshesButton : CellexalButton
    {
        protected override string Description => "Generate Meshes of Highlighted Points";



        protected override void Awake()
        {
            base.Awake();
        }

        public override void Click()
        {
            MeshGenerator.instance.GenerateMeshes();
        }
    }
}