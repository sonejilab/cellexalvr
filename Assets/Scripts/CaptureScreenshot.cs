using UnityEngine;
using UnityEngine.UI;
using System.IO;

/// <summary>
/// This class takes screenshots of what the user sees in the virtual environment.
/// </summary>
public class CaptureScreenshot : MonoBehaviour
{
    public SteamVR_TrackedObject rightController;
    public GameObject fadeScreen;
    private SteamVR_Controller.Device device;
    private float fadeTime = 0.7f;
    private float elapsedTime = 0.0f;
    private float colorAlpha;
    private int screenshotCounter;
    private string directory = Directory.GetCurrentDirectory() + "\\Screenshots";

    void Start()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        //fadeScreen.GetComponent<Image> ().color = new Color (0, 0, 0);
    }

    void Update()
    {
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
        {
            Vector2 touchpad = (device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0));
            if (touchpad.y > 0.7f)
            {
                //Touchpad 
                if (!Directory.Exists(directory))
                {
                    CellexalLog.Log("Creating directory " + CellexalLog.FixFilePath(directory));
                    Directory.CreateDirectory(directory);
                }
                ScreenCapture.CaptureScreenshot(directory + "\\Screenshot_" + System.DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png");
                CellexalLog.Log("Screenshot taken!");
                elapsedTime = 0.0f;
                screenshotCounter++;
            }
        }

        if (elapsedTime < fadeTime / 2.0f)
        {
            elapsedTime += Time.deltaTime;
            colorAlpha += 0.05f;
            fadeScreen.GetComponent<Image>().color = new Color(0, 0, 0, colorAlpha);
        }
        else if (elapsedTime < fadeTime && elapsedTime > (fadeTime / 2.0f))
        {
            elapsedTime += Time.deltaTime;
            colorAlpha -= 0.05f;
            fadeScreen.GetComponent<Image>().color = new Color(0, 0, 0, colorAlpha);
        }
    }

}
