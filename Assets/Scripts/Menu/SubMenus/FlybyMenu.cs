using CellexalVR.DesktopUI;
using CellexalVR.General;
using CellexalVR.Menu.Buttons.Flyby;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

namespace CellexalVR.Menu.SubMenus
{
    /// <summary>
    /// The submenu for creating flybys. A flyby is a (short) video where a camera pans through some positions to highlight some intersting thing in some intersting data.
    /// </summary>
    public class FlybyMenu : MenuWithoutTabs
    {
        public GameObject cameraGameObject;
        public GameObject previewSpherePrefab;
        public LineRenderer previewLine;
        public GameObject changeFlybyLineModePrefab;

        private List<Vector3> positions = new List<Vector3>();
        private List<Vector3> bezierPositions = new List<Vector3>();
        private List<Quaternion> rotations = new List<Quaternion>();
        private List<GameObject> previewSpheres = new List<GameObject>();
        private List<ChangeFlybyLineModeButton> lineModeButtons = new List<ChangeFlybyLineModeButton>();
        private RenderTexture renderTexture;
        private bool rendering = false;

        public enum FlybyRenderQuality { q1080p, q720p, q480p }
        public enum FlybyLineMode { Linear, Bezier }

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
                        renderTexture = new RenderTexture(1920, 1080, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                        break;
                    case FlybyRenderQuality.q720p:
                        renderTexture = new RenderTexture(1280, 720, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                        break;
                    case FlybyRenderQuality.q480p:
                        renderTexture = new RenderTexture(854, 480, 16, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
                        break;
                }
            }
        }

        private bool sphereGrabbed = false;
        private int sphereGrabbedIndex = -1;
        private bool checkpointSphereGrabbed = false;
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
            GameObject newSphere = Instantiate(previewSpherePrefab);
            newSphere.transform.position = position;
            newSphere.transform.rotation = rotation;
            newSphere.GetComponent<Interaction.PreviewSphereInteract>().Index = previewSpheres.Count;
            previewSpheres.Add(newSphere);

            if (positions.Count >= 2)
            {

                Vector3 halfPosition = (positions[positions.Count - 2] + position) / 2f;
                bezierPositions.Add(halfPosition);
                GameObject middleButton = Instantiate(changeFlybyLineModePrefab);
                middleButton.transform.position = halfPosition;
                ChangeFlybyLineModeButton buttonScript = middleButton.GetComponent<ChangeFlybyLineModeButton>();
                buttonScript.Index = lineModeButtons.Count;
                Interaction.PreviewSphereInteract interactScript = middleButton.GetComponent<Interaction.PreviewSphereInteract>();
                interactScript.Index = lineModeButtons.Count;
                lineModeButtons.Add(buttonScript);

                if (positions.Count == 2)
                {
                    cameraGameObject.SetActive(true);
                    previewLine.gameObject.SetActive(true);
                    previewLine.positionCount = 2;
                    previewLine.SetPositions(positions.ToArray());
                }
                else
                {
                    previewLine.positionCount += 1;
                }
                UpdateLinePosition(previewLine.positionCount - 1, position);
                RestartPreview();
            }
        }

        /// <summary>
        /// Updates a position and rotation on the path.
        /// </summary>
        /// <param name="index">The position/rotation to update.</param>
        /// <param name="newPosition">The new position.</param>
        /// <param name="newRotation">The new rotation.</param>
        public void UpdatePosition(int index, Vector3 newPosition, Quaternion newRotation, bool checkpointSphere)
        {
            if (checkpointSphere)
            {
                positions[index] = newPosition;
                rotations[index] = newRotation;
                if (index > 0 && lineModeButtons[index - 1].mode == FlybyLineMode.Linear)
                {
                    bezierPositions[index - 1] = HalfwayPosition(index - 1);
                }
                if (index < positions.Count - 1 && lineModeButtons[index].mode == FlybyLineMode.Linear)
                {
                    bezierPositions[index] = HalfwayPosition(index);
                }
                UpdateLinePosition(index, newPosition);
            }
            else
            {
                bezierPositions[index] = newPosition;
                UpdateLinePosition(index, positions[index]);
            }

        }

        /// <summary>
        /// Updates the position of the line when a sphere is moved.
        /// </summary>
        /// <param name="index">The index of the sphere</param>
        /// <param name="newPosition">The sphere's new position in world coordinates.</param>
        private void UpdateLinePosition(int index, Vector3 newPosition)
        {
            int rightLineIndex = LineIndex(index);
            previewLine.SetPosition(rightLineIndex, newPosition);
            if (index < lineModeButtons.Count && lineModeButtons[index].mode == FlybyLineMode.Bezier)
            {
                InterpolateBezierCurve(rightLineIndex, positions[index], bezierPositions[index], positions[index + 1]);
                lineModeButtons[index].UpdateBezierControlPolygon(positions[index], bezierPositions[index], positions[index + 1]);
            }

            if (index > 0 && lineModeButtons[index - 1].mode == FlybyLineMode.Bezier)
            {
                int leftLineIndex = LineIndex(index - 1);
                InterpolateBezierCurve(leftLineIndex, positions[index - 1], bezierPositions[index - 1], positions[index]);
                lineModeButtons[index - 1].UpdateBezierControlPolygon(positions[index - 1], bezierPositions[index - 1], positions[index]);
            }
        }

        /// <summary>
        /// Interpolates a quadratic bezier curve given the three coordinates of its control polygon and updates the preview line with the new bexier curve.
        /// </summary>
        /// <param name="index">The index of the first position of preview line's <see cref="LineRenderer">'s positions.</see> </param>
        /// <param name="p0">The first coordinate, where the line starts.</param>
        /// <param name="p1">The second coordinate, which the line bends towards.</param>
        /// <param name="p2">The third coordinate, where the line ends.</param>
        private void InterpolateBezierCurve(int index, Vector3 p0, Vector3 p1, Vector3 p2)
        {
            float t = 0f;
            for (int i = index; i < index + 10; ++i)
            {
                Vector3 pos = PointOnBezierCurve(p0, p1, p2, t);
                previewLine.SetPosition(i, pos);
                t += 1f / 9f;
            }
        }

        /// <summary>
        /// Calculates one position on a bezier curve.
        /// </summary>
        /// <param name="p0">The position of the first vertex of the control polygon.</param>
        /// <param name="p1">The position of the second vertex of the control polygon.</param>
        /// <param name="p2">The position of the third vertex of the control polygon.</param>
        /// <param name="t">How far along the curve to go, range [0, 1].</param>
        /// <returns>A position on the bezier curve.</returns>
        private Vector3 PointOnBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            float oneMinusT = 1f - t;
            return oneMinusT * oneMinusT * p0 + 2f * oneMinusT * t * p1 + t * t * p2;
        }

        /// <summary>
        /// Updates the mode of a line between two positions. The line on index n is between <see cref="positions"/> index n and n+1.
        /// </summary>
        /// <param name="index">The index of the line.</param>
        /// <param name="newMode">The new mode</param>
        public void UpdateLineMode(int index, FlybyLineMode newMode)
        {
            int lineIndex = LineIndex(index);
            Vector3[] oldPos = new Vector3[previewLine.positionCount];
            previewLine.GetPositions(oldPos);
            Vector3[] newPos;
            if (newMode == FlybyLineMode.Linear)
            {
                newPos = new Vector3[previewLine.positionCount - 9];
                Array.Copy(oldPos, 0, newPos, 0, lineIndex);
                Array.Copy(oldPos, lineIndex + 9, newPos, lineIndex, oldPos.Length - lineIndex - 9);

            }
            else
            {
                newPos = new Vector3[previewLine.positionCount + 9];
                Array.Copy(oldPos, 0, newPos, 0, lineIndex);
                Array.Copy(oldPos, lineIndex, newPos, lineIndex + 9, oldPos.Length - lineIndex);
            }
            previewLine.positionCount = newPos.Length;
            previewLine.SetPositions(newPos);
            UpdateLinePosition(index, positions[index]);
        }

        /// <summary>
        /// Gets the index of linerenderer's position that corresponds to the index of a checkpoint sphere.
        /// </summary>
        /// <param name="sphereIndex">The index if the checkpoint sphere.</param>
        /// <returns>The index of the position of the linerenderer.</returns>
        private int LineIndex(int sphereIndex)
        {
            int lineIndex = 0;
            for (int i = 0; i < sphereIndex; ++i)
            {
                if (lineModeButtons[i].mode == FlybyLineMode.Bezier)
                {
                    lineIndex += 10;
                }
                else
                {
                    lineIndex++;
                }
            }
            return lineIndex;
        }

        /// <summary>
        /// Returns the position halfway between a checkpoint and the next checklpoint.
        /// </summary>
        /// <param name="index">The index of the checkpoint.</param>
        /// <returns>A <see cref="Vector3"/> of the position between the points.</returns>
        public Vector3 HalfwayPosition(int index)
        {
            return (positions[index] + positions[index + 1]) / 2f;
        }

        /// <summary>
        /// Called when a sphere is grabbed. Places the camera on the sphere as long as it is grabbed.
        /// </summary>
        /// <param name="grabbed">True if the sphere was grabbed, false if it was ungrabbed.</param>
        /// <param name="sphere">The sphere that was grabbed.</param>
        /// <param name="index">The index of this sphere</param>
        /// <param name="checkpointSphere">True if <paramref name="sphere"/> is a checkpoint that the camera passes through, false if it is a <see cref="ChangeFlybyLineModeButton"/>.</param>
        public void SetSphereGrabbed(bool grabbed, GameObject sphere, int index, bool checkpointSphere)
        {
            sphereGrabbed = grabbed;
            sphereGrabbedIndex = index;
            checkpointSphereGrabbed = checkpointSphere;

            if (!sphereGrabbed)
            {
                cameraGameObject.transform.parent = null;
                RestartPreview();
            }

            if (sphereGrabbed && checkpointSphere)
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

                cameraGameObject.transform.rotation = Quaternion.Lerp(rot1, rot2, previewT);

                if (lineModeButtons[previewPositionIndex].mode == FlybyLineMode.Linear)
                {
                    cameraGameObject.transform.position = Vector3.Lerp(pos1, pos2, previewT);
                }
                else
                {
                    cameraGameObject.transform.position = PointOnBezierCurve(pos1, bezierPositions[previewPositionIndex], pos2, previewT);
                }

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

            if (sphereGrabbed && checkpointSphereGrabbed)
            {
                if (sphereGrabbedIndex < previewSpheres.Count - 1 && lineModeButtons[sphereGrabbedIndex].mode == FlybyLineMode.Linear)
                {
                    Vector3 halfPosition = (previewSpheres[sphereGrabbedIndex].transform.position + previewSpheres[sphereGrabbedIndex + 1].transform.position) / 2f;
                    lineModeButtons[sphereGrabbedIndex].transform.position = halfPosition;
                }

                if (sphereGrabbedIndex >= 1 && lineModeButtons[sphereGrabbedIndex - 1].mode == FlybyLineMode.Linear)
                {
                    Vector3 halfPosition = (previewSpheres[sphereGrabbedIndex].transform.position + previewSpheres[sphereGrabbedIndex - 1].transform.position) / 2f;
                    lineModeButtons[sphereGrabbedIndex - 1].transform.position = halfPosition;
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
            HidePreviewObjects();
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
                    cameraGameObject.transform.rotation = Quaternion.Lerp(rot1, rot2, t);

                    if (lineModeButtons[i].mode == FlybyLineMode.Linear)
                    {
                        cameraGameObject.transform.position = Vector3.Lerp(pos1, pos2, t);
                    }
                    else
                    {
                        cameraGameObject.transform.position = PointOnBezierCurve(pos1, bezierPositions[i], pos2, t);
                    }
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
            RemovePreviewObjects();
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

        public void HidePreviewObjects()
        {
            foreach (GameObject sphere in previewSpheres)
            {
                sphere.SetActive(false);
            }

            foreach (ChangeFlybyLineModeButton button in lineModeButtons)
            {
                button.gameObject.SetActive(false);
            }

            previewLine.gameObject.SetActive(false);
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

            foreach (ChangeFlybyLineModeButton button in lineModeButtons)
            {
                Destroy(button.gameObject);
            }
            lineModeButtons.Clear();

            previewLine.positionCount = 0;
        }
    }
}
