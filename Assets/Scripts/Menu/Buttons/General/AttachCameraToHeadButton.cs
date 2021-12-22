using CellexalVR.Menu.Buttons;
using Valve.VR.InteractionSystem;

namespace Menu.Buttons.General
{
    public class AttachCameraToHeadButton : CellexalButton
    {
        protected override string Description => "Flip Camera";

        public override void Click()
        {
            referenceManager.screenshotCamera.snapShotCamera.transform.parent = Player.instance.hmdTransform;
        }
    }
}