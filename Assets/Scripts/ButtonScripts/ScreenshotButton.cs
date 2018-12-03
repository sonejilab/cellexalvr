using UnityEngine;
/// <summary>
/// Represents the button that toggles the screenshot tool.
/// </summary>
public class ScreenshotButton : CellexalButton
{

    public GameObject camera;
    public GameObject canvas;

    protected override string Description
    {
        get { return "Take Snapshots"; }
    }

    protected override void Click()
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
