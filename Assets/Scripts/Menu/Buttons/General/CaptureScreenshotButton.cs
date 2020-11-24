using CellexalVR.Menu.Buttons;
using UnityEngine;

namespace Menu.Buttons.General
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