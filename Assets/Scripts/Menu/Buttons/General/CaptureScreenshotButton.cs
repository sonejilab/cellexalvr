namespace CellexalVR.Menu.Buttons.General
{
    public class CaptureScreenshotButton : CellexalButton
    {
        protected override string Description => "Capture Screenshot";

        public override void Click()
        {
            referenceManager.screenshotCamera.Capture();
        }
    }
}