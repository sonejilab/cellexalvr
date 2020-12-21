using CellexalVR.Tools;
using UnityEngine;
using Valve.VR.InteractionSystem;

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
                spriteRenderer.sprite = standardTexture;
                referenceManager.screenshotCamera.gameObject.SetActive(false);
                referenceManager.screenshotCamera.gameObject.GetComponent<CaptureScreenshot>().enabled = false;
            }
            else
            {
                spriteRenderer.sprite = deactivatedTexture;
                CaptureScreenshot screenshotCamera = referenceManager.screenshotCamera;
                screenshotCamera.gameObject.SetActive(true);
                screenshotCamera.transform.position = referenceManager.headset.transform.position + referenceManager.headset.transform.forward * 0.7f;
                screenshotCamera.transform.LookAt(referenceManager.headset.transform);
                screenshotCamera.transform.Rotate(0, 180, 0);
                screenshotCamera.gameObject.GetComponent<CaptureScreenshot>().enabled = true;
            }

        }
    }
}