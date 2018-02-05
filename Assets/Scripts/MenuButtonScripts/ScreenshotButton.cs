using UnityEngine;
using UnityEngine.SceneManagement;

public class ScreenshotButton : StationaryButton
{

    public Camera camera;
    public GameObject canvas;

    protected override string Description
    {
        get { return "Take Snapshots"; }
    }

    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
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
