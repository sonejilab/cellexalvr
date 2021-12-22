using CellexalVR.AnalysisLogic;
using CellexalVR.General;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Menu.Buttons.General;
using UnityEngine;

namespace CellexalVR.Tools
{
    /// <summary>
    /// This class takes screenshots of what the user sees in the virtual environment.
    /// </summary>
    public class CaptureScreenshot : MonoBehaviour
    {
        public Camera snapShotCamera;
        public int picWidth;
        public int picHeight;
        public GameObject quadPrefab;
        public Light flash;
        public Color backgroundColor;

        private float fadeTime = 0.7f;
        private float elapsedTime = 0.0f;
        private float colorAlpha;
        private int screenshotCounter;
        private ScreenshotLayerToggleButton[] layerButtons = new ScreenshotLayerToggleButton[] { };
        private readonly List<string> layersToRender = new List<string>();
        private AudioSource audioSource;

        private void Start()
        {
            layerButtons = GetComponentsInChildren<ScreenshotLayerToggleButton>();
            // foreach (ScreenshotLayerToggleButton button in layerButtons)
            // {
            //     button.CurrentState = true;
            // }

            gameObject.SetActive(false);
            audioSource = GetComponent<AudioSource>();
        }

        public void Capture()
        {
            StartCoroutine(CaptureCoroutine());
        }

        /// <summary>
        /// Method to capture the screenshot. Renders the pixels from the screenshot camera. Saves it to the screenshot directory. 
        /// </summary>
        /// <returns></returns>
        private IEnumerator CaptureCoroutine()
        {
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
            flash.enabled = true;
            yield return null;
            Texture2D snapTex = new Texture2D(picWidth, picHeight, TextureFormat.ARGB32, false);

            RenderTexture.active = snapShotCamera.targetTexture;
            snapTex.ReadPixels(snapShotCamera.pixelRect, 0, 0);
            snapTex.Apply();
            flash.enabled = false;


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
            byte[] image = snapTex.EncodeToPNG();
            File.WriteAllBytes(screenshotImageFilePath, image);

            GameObject quad = Instantiate(quadPrefab, transform);
            quad.SetActive(true);
            quad.GetComponent<MeshRenderer>().material.mainTexture = snapTex;
            yield return new WaitForSeconds(1.5f);
            Destroy(snapTex);
            Destroy(quad);
        }

        public void ToggleLayerToCapture(string layerName, bool toggle)
        {
            if (toggle && !layersToRender.Contains(layerName))
            {
                layersToRender.Add(layerName);
            }
            else
            {
                layersToRender.Remove(layerName);
            }

            snapShotCamera.cullingMask = LayerMask.GetMask(layersToRender.ToArray());
        }

        public void ToggleBackground(bool toggle)
        {
            if (toggle)
            {
                snapShotCamera.clearFlags = CameraClearFlags.Skybox;
            }

            else
            {
                snapShotCamera.clearFlags = CameraClearFlags.SolidColor;
                snapShotCamera.backgroundColor = backgroundColor;
            }
        }

        public void ToggleAllLayers(bool toggle)
        {
            foreach (ScreenshotLayerToggleButton button in layerButtons)
            {
                button.CurrentState = toggle;
            }

            // first 8 layers are the built in ones. unity allows 32 layers max.
            for (int i = 0; i < 8; i++)
            {
                string layerName = LayerMask.LayerToName(i);
                ToggleLayerToCapture(layerName, toggle);
            }
        }

        public void Toggle(bool toggle)
        {
            gameObject.SetActive(toggle);
            if (toggle)
            {
                // Transform cameraPosition = Player.instance.headCollider.transform;
                transform.localPosition = ReferenceManager.instance.headset.transform.position + ReferenceManager.instance.headset.transform.forward * 0.7f;
                transform.LookAt(ReferenceManager.instance.headset.transform.position);
                transform.Rotate(0, 180, 0);
            }
        }

        /// <summary>
        /// Calls R logging function to save screenshot for session report.
        /// </summary
        private IEnumerator LogScreenshot(string screenshotImageFilePath)
        {
            string args = screenshotImageFilePath;
            string rScriptFilePath = Application.streamingAssetsPath + @"\R\screenshot_report.R";

            bool rServerReady = File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.pid") &&
                                !File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.input.R") &&
                                !File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.input.lock");
            while (!rServerReady || !RScriptRunner.serverIdle)
            {
                rServerReady = File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.pid") &&
                               !File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.input.R") &&
                               !File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.input.lock");
                yield return null;
            }

            //t = new Thread(() => RScriptRunner.RunScript(script));
            Thread t = new Thread(() => RScriptRunner.RunRScript(rScriptFilePath, args));
            t.Start();
            CellexalLog.Log("Running R function " + rScriptFilePath + " with the arguments: " + args);
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            while (t.IsAlive || File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.input.R"))
            {
                yield return null;
            }

            stopwatch.Stop();
            CellexalLog.Log("R log script finished in " + stopwatch.Elapsed.ToString());
        }
    }
}