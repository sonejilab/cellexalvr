using CellexalVR.Tools;
using UnityEngine;
namespace CellexalVR.Menu.Buttons.Tools
{
    /// <summary>
    /// Represents the button that toggles the screenshot tool.
    /// </summary>
    public class ScreenshotButton : CellexalButton
    { 
        //public GameObject canvas;

        protected override string Description
        {
            get { return "Take Snapshots"; }
        }

        public override void Click()
        {
            if (referenceManager.screenshotCamera.gameObject.activeSelf)
            {
                //canvas.SetActive(false);
                spriteRenderer.sprite = standardTexture;
                referenceManager.screenshotCamera.gameObject.SetActive(false);
                referenceManager.screenshotCamera.gameObject.GetComponent<CaptureScreenshot>().enabled = false;
                // referenceManager.screenCanvas.gameObject.SetActive(false);
            }
            else
            {
                //canvas.SetActive(true);
                spriteRenderer.sprite = deactivatedTexture;
                referenceManager.screenshotCamera.gameObject.SetActive(true);
                referenceManager.screenshotCamera.gameObject.GetComponent<CaptureScreenshot>().enabled = true;
                // referenceManager.screenCanvas.gameObject.SetActive(true);

            }

        }
    }
}