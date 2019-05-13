using CellexalVR.Tools;
using UnityEngine;
namespace CellexalVR.Menu.Buttons.Tools
{
    /// <summary>
    /// Represents the button that toggles the screenshot tool.
    /// </summary>
    public class ScreenshotButton : CellexalButton
    {

        public new GameObject camera;
        public GameObject canvas;

        protected override string Description
        {
            get { return "Take Snapshots"; }
        }

        public override void Click()
        {
            if (canvas.activeSelf)
            {
                canvas.SetActive(false);
                spriteRenderer.sprite = standardTexture;
                camera.gameObject.SetActive(false);
            }
            else
            {
                canvas.SetActive(true);
                spriteRenderer.sprite = deactivatedTexture;
                camera.gameObject.SetActive(true);
            }

            camera.gameObject.GetComponent<CaptureScreenshot>().enabled = !camera.gameObject.GetComponent<CaptureScreenshot>().enabled;
        }
    }
}