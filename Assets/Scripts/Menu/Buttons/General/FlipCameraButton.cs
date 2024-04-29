namespace CellexalVR.Menu.Buttons.General
{
    public class FlipCameraButton : CellexalButton
    {
        protected override string Description => "Flip Camera";

        public override void Click()
        {
            referenceManager.screenshotCamera.snapShotCamera.transform.Rotate(0, 180, 0);
        }
    }
}