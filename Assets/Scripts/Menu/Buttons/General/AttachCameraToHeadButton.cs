using CellexalVR.General;

namespace CellexalVR.Menu.Buttons.General
{
    public class AttachCameraToHeadButton : CellexalButton
    {
        protected override string Description => "Flip Camera";

        public override void Click()
        {
            referenceManager.screenshotCamera.snapShotCamera.transform.parent = ReferenceManager.instance.headset.transform;
        }
    }
}