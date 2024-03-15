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
            foreach (ScreenshotLayerToggleButton button in layerButtons)
            {
                foreach (string layerName in button.layerNames)
                {
                    if (layerName != "Background" && !button.toggleAllButton)
                    {
                        layersToRender.Add(layerName);
                    }
                }
            }
            snapShotCamera.cullingMask = LayerMask.GetMask(layersToRender.ToArray());

            gameObject.SetActive(false);
            audioSource = GetComponent<AudioSource>();
        }

        /// <summary>
        /// Takes a screenshot of what is currently on the rendertexture.
        /// </summary>
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

            screenshotImageDirectory = Path.Combine(screenshotImageDirectory, "Screenshots");
            if (!Directory.Exists(screenshotImageDirectory))
            {
                Directory.CreateDirectory(screenshotImageDirectory);
                CellexalLog.Log("Created directory " + screenshotImageDirectory);
            }

            string screenshotImageFilePath = Path.Combine(screenshotImageDirectory, name + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png");
            byte[] image = snapTex.EncodeToPNG();
            File.WriteAllBytes(screenshotImageFilePath, image);

            GameObject quad = Instantiate(quadPrefab, transform);
            quad.SetActive(true);
            quad.GetComponent<MeshRenderer>().material.mainTexture = snapTex;
            yield return new WaitForSeconds(1.5f);
            Destroy(snapTex);
            Destroy(quad);
        }

        /// <summary>
        /// Toggles a layer to render on the rendertexture.
        /// </summary>
        /// <param name="layerName">The name of the layer to toggle.</param>
        /// <param name="toggle">True to toggle the layer on, false to toggle off.</param>
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

        /// <summary>
        /// Toggles rendering the skybox on the rendertexture.
        /// </summary>
        /// <param name="toggle">True to toggle the skybox on, false to toggle off.</param>
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

        /// <summary>
        /// Toggles all layers to render on the rendertexture.
        /// </summary>
        /// <param name="toggle">True to toggle the layers on, false to toggle off.</param>
        public void ToggleAllLayers(bool toggle)
        {
            foreach (ScreenshotLayerToggleButton button in layerButtons)
            {
                button.CurrentState = toggle;
            }
        }

        /// <summary>
        /// Toggles this gameobject on or off. Repositions it in front of the user when toggled on.
        /// </summary>
        /// <param name="toggle">True for toggling in, false for toggling off.</param>
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
            string rScriptFilePath = Path.Combine(Application.streamingAssetsPath, "R", "screenshot_report.R");

            string mainserverPidPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.pid");
            string mainserverInputPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.R");
            string mainserverInputLockPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.lock");

            bool rServerReady = File.Exists(mainserverPidPath)
                                && !File.Exists(mainserverInputPath)
                                && !File.Exists(mainserverInputLockPath);
            while (!rServerReady || !RScriptRunner.serverIdle)
            {
                rServerReady = File.Exists(mainserverPidPath)
                               && !File.Exists(mainserverInputPath)
                               && !File.Exists(mainserverInputLockPath);
                yield return null;
            }

            //t = new Thread(() => RScriptRunner.RunScript(script));
            Thread t = new Thread(() => RScriptRunner.RunRScript(rScriptFilePath, args));
            t.Start();
            CellexalLog.Log("Running R function " + rScriptFilePath + " with the arguments: " + args);
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            while (t.IsAlive || File.Exists(mainserverInputPath))
            {
                yield return null;
            }

            stopwatch.Stop();
            CellexalLog.Log("R log script finished in " + stopwatch.Elapsed.ToString());
        }
    }
}