﻿using CellexalVR.AnalysisObjects;
using CellexalVR.DesktopUI;
using CellexalVR.Extensions;
using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using UnityEngine;

namespace CellexalVR.AnalysisLogic
{
    /// <summary>
    /// A generator for heatmaps. Creates the colors that are later used when generating heatmaps
    /// </summary>
    public class HeatmapGenerator : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject heatmapPrefab;
        public int selectionNr;

        public bool GeneratingHeatmaps { get; private set; }

        public SolidBrush[] expressionColors;


        private GameObject calculatorCluster;
        public SelectionToolHandler selectionToolHandler;
        //private StatusDisplay status;
        //private StatusDisplay statusDisplayHUD;
        //private StatusDisplay statusDisplayFar;
        private ArrayList data;
        private Thread t;
        private SteamVR_Controller.Device device;
        private Vector3 heatmapPosition;
        private List<Heatmap> heatmapList = new List<Heatmap>();
        private string statsMethod;
        public UnityEngine.Color HighlightMarkerColor { get; private set; }
        public UnityEngine.Color ConfirmMarkerColor { get; private set; }

        void Awake()
        {
            t = null;
            heatmapPosition = heatmapPrefab.transform.position;
            selectionToolHandler = referenceManager.selectionToolHandler;
            //status = referenceManager.statusDisplay;
            //statusDisplayHUD = referenceManager.statusDisplayHUD;
            //statusDisplayFar = referenceManager.statusDisplayFar;
            calculatorCluster = referenceManager.calculatorCluster;
            calculatorCluster.SetActive(false);
            GeneratingHeatmaps = false;
            CellexalEvents.ConfigLoaded.AddListener(InitColors);
        }

        /// <summary>
        /// Initializes <see cref="expressionColors"/> with the colors in the config file.
        /// </summary>
        public void InitColors()
        {

            int numberOfExpressionColors = CellexalConfig.Config.NumberOfHeatmapColors;
            expressionColors = new SolidBrush[numberOfExpressionColors];
            UnityEngine.Color low = CellexalConfig.Config.HeatmapLowExpressionColor;
            UnityEngine.Color mid = CellexalConfig.Config.HeatmapMidExpressionColor;
            UnityEngine.Color high = CellexalConfig.Config.HeatmapHighExpressionColor;
            //print(low + " " + mid + " " + high);

            int dividerLowMid = numberOfExpressionColors / 2;
            if (dividerLowMid == 0)
                dividerLowMid = 1;
            float lowMidDeltaR = (mid.r * mid.r - low.r * low.r) / dividerLowMid;
            float lowMidDeltaG = (mid.g * mid.g - low.g * low.g) / dividerLowMid;
            float lowMidDeltaB = (mid.b * mid.b - low.b * low.b) / dividerLowMid;

            int dividerMidHigh = numberOfExpressionColors - dividerLowMid - 1;
            if (dividerMidHigh == 0)
                dividerMidHigh = 1;
            float midHighDeltaR = (high.r * high.r - mid.r * mid.r) / dividerMidHigh;
            float midHighDeltaG = (high.g * high.g - mid.g * mid.g) / dividerMidHigh;
            float midHighDeltaB = (high.b * high.b - mid.b * mid.b) / dividerMidHigh;
            //print(midHighDeltaR + " " + midHighDeltaG + " " + midHighDeltaB);

            for (int i = 0; i < numberOfExpressionColors / 2 + 1; ++i)
            {
                float r = low.r * low.r + lowMidDeltaR * i;
                float g = low.g * low.g + lowMidDeltaG * i;
                float b = low.b * low.b + lowMidDeltaB * i;
                if (r < 0) r = 0;
                if (g < 0) g = 0;
                if (b < 0) b = 0;
                expressionColors[i] = new SolidBrush(System.Drawing.Color.FromArgb((int)(Mathf.Sqrt(r) * 255), (int)(Mathf.Sqrt(g) * 255), (int)(Mathf.Sqrt(b) * 255)));
            }
            for (int i = numberOfExpressionColors / 2 + 1, j = 1; i < numberOfExpressionColors; ++i, ++j)
            {
                float r = mid.r * mid.r + midHighDeltaR * j;
                float g = mid.g * mid.g + midHighDeltaG * j;
                float b = mid.b * mid.b + midHighDeltaB * j;
                if (r < 0) r = 0;
                if (g < 0) g = 0;
                if (b < 0) b = 0;
                expressionColors[i] = new SolidBrush(System.Drawing.Color.FromArgb((int)(Mathf.Sqrt(r) * 255), (int)(Mathf.Sqrt(g) * 255), (int)(Mathf.Sqrt(b) * 255)));
            }
            HighlightMarkerColor = CellexalConfig.Config.HeatmapHighlightMarkerColor;
            ConfirmMarkerColor = CellexalConfig.Config.HeatmapConfirmMarkerColor;
        }


        internal void DeleteHeatmaps()
        {
            foreach (Heatmap h in heatmapList)
            {
                if (h != null)
                {
                    Destroy(h.gameObject);
                }
            }
            heatmapList.Clear();
        }

        [ConsoleCommand("heatmapGenerator", "generateheatmap", "gh")]
        public void CreateHeatmap()
        {
            statsMethod = CellexalConfig.Config.HeatmapAlgorithm;
            CellexalLog.Log("Creating heatmap");
            CellexalEvents.CreatingHeatmap.Invoke();
            string heatmapName = "heatmap_" + System.DateTime.Now.ToString("HH-mm-ss");
            StartCoroutine(GenerateHeatmapRoutine(heatmapName));
        }

        /// <summary>
        /// Creates a new heatmap using the last confirmed selection.
        /// </summary>
        /// <param name="name">If created via multiplayer. Name it the same as on other client.</param>
        public void CreateHeatmap(string name = "")
        {
            // name the heatmap "heatmap_X". Where X is some number.
            string heatmapName = "";
            if (name.Equals(string.Empty))
            {
                heatmapName = "heatmap_" + System.DateTime.Now.ToString("HH-mm-ss");
                referenceManager.gameManager.InformCreateHeatmap(heatmapName);
            }
            else
            {
                heatmapName = name;
            }
            statsMethod = CellexalConfig.Config.HeatmapAlgorithm;
            CellexalLog.Log("Creating heatmap");
            CellexalEvents.CreatingHeatmap.Invoke();
            StartCoroutine(GenerateHeatmapRoutine(heatmapName));
        }

        public Heatmap FindHeatmap(string heatmapName)
        {
            foreach (Heatmap hm in heatmapList)
            {
                if (hm.name == heatmapName)
                {
                    return hm;
                }
            }
            return null;
        }

        /// <summary>
        /// Coroutine for creating a heatmap.
        /// </summary>
        IEnumerator GenerateHeatmapRoutine(string heatmapName)
        {
            if (selectionToolHandler.selectionConfirmed)
            {
                GeneratingHeatmaps = true;
                // Show calculators
                calculatorCluster.SetActive(true);
                List<Graph.GraphPoint> selection = selectionToolHandler.GetLastSelection();

                // Check if more than one cell is selected
                if (selection.Count < 1)
                {
                    CellexalLog.Log("can not create heatmap with less than 1 graphpoints, aborting");
                    yield break;
                }

                //int statusId = status.AddStatus("R script generating heatmap");
                //int statusIdHUD = statusDisplayHUD.AddStatus("R script generating heatmap");
                //int statusIdFar = statusDisplayFar.AddStatus("R script generating heatmap");

                // if the R object is not updated, wait
                while (selectionToolHandler.RObjectUpdating)
                    yield return null;

                // Start generation of new heatmap in R
                selectionNr = selectionToolHandler.fileCreationCtr - 1;
                //string home = Directory.GetCurrentDirectory();

                string rScriptFilePath = (Application.streamingAssetsPath + @"\R\make_heatmap.R").FixFilePath();
                string heatmapDirectory = (CellexalUser.UserSpecificFolder + @"\Heatmap").FixFilePath();
                string outputFilePath = (heatmapDirectory + @"\" + heatmapName + ".txt").FixFilePath();
                string args = heatmapDirectory + " " + CellexalUser.UserSpecificFolder + " " + selectionNr + " " + outputFilePath +
                                " " + CellexalConfig.Config.HeatmapNumberOfGenes + " " + statsMethod;
                if (!Directory.Exists(heatmapDirectory))
                {
                    CellexalLog.Log("Creating directory " + heatmapDirectory.FixFilePath());
                    Directory.CreateDirectory(heatmapDirectory);
                }
                CellexalLog.Log("Running R script " + rScriptFilePath.FixFilePath() + " with the arguments \"" + args + "\"");
                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
                t = new Thread(() => RScriptRunner.RunFromCmd(rScriptFilePath, args));
                t.Start();

                while (t.IsAlive)
                {
                    yield return null;
                }
                stopwatch.Stop();
                CellexalLog.Log("Heatmap R script finished in " + stopwatch.Elapsed.ToString());
                //status.RemoveStatus(statusId);
                //statusDisplayHUD.RemoveStatus(statusIdHUD);
                //statusDisplayFar.RemoveStatus(statusIdFar);
                GeneratingHeatmaps = false;
                //File.Delete(newHeatmapFilePath);
                //File.Move(heatmapFilePath + @"\heatmap.png", newHeatmapFilePath);

                var heatmap = Instantiate(heatmapPrefab).GetComponent<Heatmap>();
                heatmap.Init();
                heatmap.transform.parent = transform;
                heatmap.transform.localPosition = heatmapPosition;
                heatmap.selectionNr = selectionNr;
                // save colors before.
                //heatmap.SetVars(colors);
                heatmapList.Add(heatmap);
                //heatmap.UpdateImage(newHeatmapFilePath);
                heatmap.BuildTexture(selection, outputFilePath);
                //heatmap.GetComponent<AudioSource>().Play();
                heatmap.name = heatmapName; //"heatmap_" + heatmapsCreated;
                heatmap.highlightQuad.GetComponent<Renderer>().material.color = HighlightMarkerColor;
                heatmap.confirmQuad.GetComponent<Renderer>().material.color = ConfirmMarkerColor;
            }
        }

        public void AddHeatmapToList(Heatmap heatmap)
        {
            heatmapList.Add(heatmap);
            heatmap.selectionNr = selectionNr;
        }

    }
}