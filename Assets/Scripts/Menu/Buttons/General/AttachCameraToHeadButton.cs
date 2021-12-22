using CellexalVR.General;
using CellexalVR.Menu.Buttons;

namespace Menu.Buttons.General
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