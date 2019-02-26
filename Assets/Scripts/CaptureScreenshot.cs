using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Threading;
using System.Collections;

/// <summary>
/// This class takes screenshots of what the user sees in the virtual environment.
/// </summary>
public class CaptureScreenshot : MonoBehaviour
{
    public SteamVR_TrackedObject rightController;
    public GameObject fadeScreen;
    public GameObject panel;
    private SteamVR_Controller.Device device;
    private float fadeTime = 0.7f;
    private float elapsedTime = 0.0f;
    private float colorAlpha;
    private int screenshotCounter;
    //private string directory = Directory.GetCurrentDirectory() + "\\Output\";

    void Start()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        //fadeScreen.GetComponent<Image> ().color = new Color (0, 0, 0);
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        //Vector2 touchpad = (device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0));
        if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            string screenshotImageDirectory = CellexalUser.UserSpecificFolder;
            if (!Directory.Exists(screenshotImageDirectory))
            {
                Directory.CreateDirectory(screenshotImageDirectory);
                CellexalLog.Log("Created directory " + screenshotImageDirectory);
            }

            screenshotImageDirectory += "\\Screenshots";
            if (!Directory.Exists(screenshotImageDirectory))
            {
                Directory.CreateDirectory(screenshotImageDirectory);
                CellexalLog.Log("Created directory " + screenshotImageDirectory);
            }

            string screenshotImageFilePath = screenshotImageDirectory + "\\" + name + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png";
            //Touchpad 
            //if (!Directory.Exists(directory))
            //{
            //    CellexalLog.Log("Creating directory " + CellexalLog.FixFilePath(directory));
            //    Directory.CreateDirectory(directory);
            //}
            panel.SetActive(false);
            ScreenCapture.CaptureScreenshot(screenshotImageFilePath);
            CellexalLog.Log("Screenshot taken!");
            //StartCoroutine(LogScreenshot(screenshotImageFilePath));
            elapsedTime = 0.0f;
            screenshotCounter++;
            

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
            panel.SetActive(true);
        }
    }

    /// <summary>
    /// Calls R logging function to save screenshot for session report.
    /// </summary

    IEnumerator LogScreenshot(string screenshotImageFilePath)
    {
        string args = screenshotImageFilePath;
        string rScriptFilePath = Application.streamingAssetsPath + @"\R\screenshot_report.R";
        CellexalLog.Log("Running R script " + CellexalLog.FixFilePath(rScriptFilePath) + " with the arguments \"" + args + "\"");
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        Thread t = new Thread(() => RScriptRunner.RunFromCmd(rScriptFilePath, args));
        t.Start();

        while (t.IsAlive)
        {
            yield return null;
        }
        stopwatch.Stop();
        CellexalLog.Log("R log script finished in " + stopwatch.Elapsed.ToString());
    }

}
