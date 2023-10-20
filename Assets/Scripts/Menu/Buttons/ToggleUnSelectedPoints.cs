using CellexalVR.AnalysisLogic;

namespace CellexalVR.Menu.Buttons
{
    public class ToggleUnSelectedPoints : SliderButton
    {
        protected override string Description => "Hide/Show all uncoloured graph points";

        private bool hiding;

        protected override void ActionsAfterSliding()
        {
            hiding = !hiding;
            foreach (PointCloud pc in PointCloudGenerator.instance.pointClouds)
            {
                pc.SetAlphaClipThreshold(hiding);
            }
        }
    }
}