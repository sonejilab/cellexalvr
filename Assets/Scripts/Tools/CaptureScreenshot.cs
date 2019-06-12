using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Threading;
using System.Collections;
using CellexalVR.General;
using CellexalVR.AnalysisLogic;
using CellexalVR.SceneObjects;

namespace CellexalVR.Tools
{

    /// <summary>
    /// This class takes screenshots of what the user sees in the virtual environment.
    /// </summary>
    public class CaptureScreenshot : MonoBehaviour
    {
        public SteamVR_TrackedObject rightController;
        public ScreenCanvas screenCanvas;
        public GameObject panel;

        private SteamVR_Controller.Device device;
        private float fadeTime = 0.7f;
        private float elapsedTime = 0.0f;
        private float colorAlpha;
        private int screenshotCounter;
        public ReferenceManager referenceManager;
        //private string directory = Directory.GetCurrentDirectory() + "\\Output\";

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        void Start()
        {
            device = SteamVR_Controller.Input((int)rightController.index);

            screenCanvas = referenceManager.screenCanvas;
        }

        void Update()
        {
            device = SteamVR_Controller.Input((int)rightController.index);
            //Vector2 touchpad = (device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0));
            if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                StartCoroutine(Capture());
            }

        }

        /// <summary>
        /// Method to capture screenshot. Disables the UI canvas so it does not show up on the image.
        /// </summary>
        /// <returns></returns>
        IEnumerator Capture()
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

            panel.SetActive(false);
            ScreenCapture.CaptureScreenshot(screenshotImageFilePath);
            CellexalLog.Log("Screenshot taken!");

            screenCanvas.FadeAnimation(1f);
            yield return new WaitForSeconds(0.5f);
            panel.SetActive(true);
            screenshotCounter++;
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
            Thread t = new Thread(() => RScriptRunner.RunRScript(rScriptFilePath, args));
            t.Start();

            while (t.IsAlive)
            {
                yield return null;
            }
            stopwatch.Stop();
            CellexalLog.Log("R log script finished in " + stopwatch.Elapsed.ToString());
        }

    }
}
