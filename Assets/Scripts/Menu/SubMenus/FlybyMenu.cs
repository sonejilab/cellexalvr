using CellexalVR.DesktopUI;
using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using UnityEngine;

namespace CellexalVR.Menu.SubMenus
{
    /// <summary>
    /// The submenu for creating flybys. A flyby is a (short) video where a camera pans through some positions to highlight some intersting thing in some intersting data.
    /// </summary>
    public class FlybyMenu : MenuWithoutTabs
    {
        public GameObject cameraGameObject;
        public GameObject previewSphere;
        public LineRenderer previewLine;

        private List<Vector3> positions = new List<Vector3>();
        private List<Quaternion> rotations = new List<Quaternion>();
        private List<GameObject> previewSpheres = new List<GameObject>();
        RenderTexture renderTexture;
        private bool rendering = false;

        public enum FlybyRenderQuality { q1080p, q720p, q480p }

        private FlybyRenderQuality renderQuality;
        public FlybyRenderQuality RenderQuality
        {
            get => renderQuality;
            set
            {
                renderQuality = value;
                switch (value)
                {
                    case FlybyRenderQuality.q1080p:
                        renderTexture.width = 1920;
                        renderTexture.height = 1080;
                        break;
                    case FlybyRenderQuality.q720p:
                        renderTexture.width = 1280;
                        renderTexture.height = 720;
                        break;
                    case FlybyRenderQuality.q480p:
                        renderTexture.width = 854;
                        renderTexture.height = 480;
                        break;
                }
            }
        }


        //private int captureWidth = 1280;
        //private int captureHeight = 720;
        private bool sphereGrabbed = false;
        private int framesPerSecond = 24;
        private int framesPerPos = 96;
        private int previewPositionIndex = 0;
        private float previewT = 0;
        private float tInc;

        public void Start()
        {
            renderTexture = new RenderTexture(1920, 1080, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            cameraGameObject.GetComponentInChildren<Camera>(true).targetTexture = renderTexture;
            cameraGameObject.GetComponentInChildren<Camera>(true).pixelRect = new Rect(0, 0, 1920, 1080);
            transform.Find("Preview Video").GetComponent<Renderer>().sharedMaterial.mainTexture = renderTexture;

            tInc = 1f / (framesPerPos - 1);
            cameraGameObject.transform.parent = null;
            cameraGameObject.transform.localScale = new Vector3(10f, 10f, 10f);
            cameraGameObject.GetComponent<MeshRenderer>().enabled = true;
        }

        /// <summary>
        /// Record a new position and add it to the path.
        /// </summary>
        /// <param name="position">The new position.</param>
        /// <param name="rotation">The camera's rotation at this position.</param>
        public void RecordPosition(Vector3 position, Quaternion rotation)
        {
            if (positions.Count >= 10417)
            {
                CellexalLog.Log("Can not have more than 10417 recorded positions in a flyby.");
                return;
            }
            positions.Add(position);
            rotations.Add(rotation);
            GameObject newSphere = Instantiate(previewSphere);
            newSphere.transform.position = position;
            newSphere.transform.rotation = rotation;
            newSphere.GetComponent<Interaction.PreviewSphereInteract>().Index = positions.Count - 1;
            previewSpheres.Add(newSphere);

            if (positions.Count >= 2)
            {
                cameraGameObject.SetActive(true);
                previewLine.gameObject.SetActive(true);
                previewLine.positionCount = positions.Count;
                previewLine.SetPositions(positions.ToArray());
            }
            RestartPreview();
        }

        /// <summary>
        /// Updates a position and rotation on the path.
        /// </summary>
        /// <param name="index">The position/rotation to update.</param>
        /// <param name="newPosition">The new position.</param>
        /// <param name="newRotation">The new rotation.</param>
        public void UpdatePosition(int index, Vector3 newPosition, Quaternion newRotation)
        {
            positions[index] = newPosition;
            rotations[index] = newRotation;
            previewLine.SetPosition(index, newPosition);
        }

        /// <summary>
        /// Called when a sphere is grabbed. Places the camera on the sphere as long as it is grabbed.
        /// </summary>
        /// <param name="grabbed">True if the sphere was grabbed, false if it was ungrabbed.</param>
        /// <param name="sphere">The sphere that was grabbed.</param>
        public void SetSphereGrabbed(bool grabbed, GameObject sphere)
        {
            sphereGrabbed = grabbed;

            if (!sphereGrabbed)
            {
                cameraGameObject.transform.parent = null;
                RestartPreview();
            }
            else
            {
                cameraGameObject.transform.parent = sphere.transform;
                cameraGameObject.transform.localPosition = Vector3.zero;
                cameraGameObject.transform.localRotation = Quaternion.identity;


            }
        }

        /// <summary>
        /// Restarts the camera preview.
        /// </summary>
        private void RestartPreview()
        {
            previewPositionIndex = 0;
            previewT = 0f;
        }

        [ConsoleCommand("flybyMenu", aliases: new string[] { "rdf" })]
        public void RenderDebugFlyby()
        {
            if (positions.Count == 0)
            {
                RecordPosition(new Vector3(0f, 1f, 0f), Quaternion.Euler(0f, -90f, 0f));
                RecordPosition(new Vector3(1f, 1f, 0f), Quaternion.Euler(0f, -90f, 0f));
            }
            RenderFlyby();
        }

        public void Update()
        {
            if (!rendering && !sphereGrabbed && positions.Count >= 2)
            {

                Vector3 pos1 = positions[previewPositionIndex];
                Vector3 pos2 = positions[previewPositionIndex + 1];

                Quaternion rot1 = rotations[previewPositionIndex];
                Quaternion rot2 = rotations[previewPositionIndex + 1];

                cameraGameObject.transform.position = Vector3.Lerp(pos1, pos2, previewT);
                cameraGameObject.transform.rotation = Quaternion.Lerp(rot1, rot2, previewT);

                if (previewT >= 1f)
                {
                    if (previewPositionIndex < positions.Count - 2)
                    {
                        previewPositionIndex++;
                    }
                    else
                    {
                        previewPositionIndex = 0;
                    }
                    previewT = 0f;
                }
                else
                {
                    previewT += Time.deltaTime / 4f;
                }
            }
        }

        /// <summary>
        /// Renders the currently specified flyby.
        /// </summary>
        public void RenderFlyby()
        {
            StartCoroutine(RenderFlybyCoroutine());
        }

        private IEnumerator RenderFlybyCoroutine()
        {
            rendering = true;
            RemovePreviewObjects();
            // create the necessary folders
            CellexalLog.Log("Started creating flyby");
            string flybyDir = CellexalUser.UserSpecificFolder + @"\Flyby\";
            if (Directory.Exists(flybyDir))
            {
                Directory.CreateDirectory(flybyDir);
                CellexalLog.Log("Created directory " + flybyDir);
            }

            string outputDir = flybyDir + @"\Flyby_temp";
            Directory.CreateDirectory(outputDir);
            Light headsetLight = referenceManager.headset.GetComponentInChildren<Light>();
            if (headsetLight != null)
            {
                headsetLight.enabled = false;
            }
            cameraGameObject.SetActive(true);
            Camera camera = cameraGameObject.GetComponentInChildren<Camera>();

            Texture2D frame = new Texture2D(renderTexture.width, renderTexture.height, TextureFormat.ARGB32, false, true);
            Rect rect = new Rect(0, 0, renderTexture.width, renderTexture.height);

            int fileId = 0;
            camera.targetTexture = renderTexture;
            WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();

            for (int i = 0; i < positions.Count - 1; ++i)
            {
                Vector3 pos1 = positions[i];
                Vector3 pos2 = positions[i + 1];

                Quaternion rot1 = rotations[i];
                Quaternion rot2 = rotations[i + 1];

                float t = 0;
                for (int frameIndex = 0; frameIndex < framesPerPos; t += tInc, ++frameIndex)
                {
                    yield return waitForEndOfFrame;
                    cameraGameObject.transform.position = Vector3.Lerp(pos1, pos2, t);
                    cameraGameObject.transform.rotation = Quaternion.Lerp(rot1, rot2, t);

                    RenderTexture.active = renderTexture;
                    camera.Render();
                    frame.ReadPixels(rect, 0, 0, false);
                    frame.Apply(false);

                    byte[] frameData = frame.EncodeToJPG();

                    string fileName = "frame_" + fileId.ToString("D6") + ".jpg";

                    FileStream fileStream = File.Create(outputDir + @"\" + fileName);
                    fileStream.Write(frameData, 0, frameData.Length);
                    fileStream.Flush();
                    fileStream.Close();

                    fileId++;
                }
            }
            string time = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            string outputPath = flybyDir + "\\flyby_" + time + ".mp4";
            while (File.Exists(outputPath))
            {
                // this will likely never happen
                outputPath += "_d";
            }

            using (Process ffmpegProcess = new Process())
            {
                ProcessStartInfo startInfo = ffmpegProcess.StartInfo;
                startInfo.FileName = Directory.GetCurrentDirectory() + "\\Assets\\bin\\ffmpeg.exe";
                startInfo.Arguments = "-framerate 24 -start_number 0 -i " + outputDir + "\\frame_%06d.jpg -frames:v " + fileId + " " + outputPath;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                startInfo.RedirectStandardOutput = true;
                startInfo.RedirectStandardError = true;
                ffmpegProcess.EnableRaisingEvents = true;
                ffmpegProcess.OutputDataReceived += OutputDataReceived;
                ffmpegProcess.ErrorDataReceived += OutputDataReceived;

                ffmpegProcess.Start();
                ffmpegProcess.BeginOutputReadLine();
                ffmpegProcess.BeginErrorReadLine();

                while (!ffmpegProcess.HasExited)
                {
                    ffmpegProcess.WaitForExit(10);
                    yield return null;
                }

                CellexalLog.Log("FFmpeg exited after " + ffmpegProcess.TotalProcessorTime.ToString());
                ffmpegProcess.Close();
            }

            if (headsetLight != null)
            {
                headsetLight.enabled = true;
            }

            // clean up temporary files
            foreach (string file in Directory.GetFiles(outputDir))
            {
                File.Delete(file);
            }

            Directory.Delete(outputDir);
            rendering = false;
            camera.targetTexture = renderTexture;
            cameraGameObject.SetActive(false);
            ResetPositions();
            CellexalLog.Log("Finished creating flyby");
        }

        private void OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            CellexalLog.Log(e.Data);
        }

        /// <summary>
        /// Resets all positions and stops the preview.
        /// </summary>
        public void ResetPositions()
        {
            positions.Clear();
            rotations.Clear();
            RemovePreviewObjects();
            cameraGameObject.SetActive(false);
        }

        /// <summary>
        /// Removes the preview related gamebobjects.
        /// </summary>
        public void RemovePreviewObjects()
        {
            foreach (GameObject sphere in previewSpheres)
            {
                Destroy(sphere);
            }

            previewSpheres.Clear();
            previewLine.positionCount = 0;
        }
    }
}
