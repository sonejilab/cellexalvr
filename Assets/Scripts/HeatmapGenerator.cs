using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Threading;
using UnityEngine;

/// <summary>
/// A generator for heatmaps. Creates the colors that are later used when generating heatmaps
/// </summary>
public class HeatmapGenerator : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public GameObject heatmapPrefab;
    public ErrorMessageController errorMessageController;

    public bool GeneratingHeatmaps { get; private set; }

    public SolidBrush[] expressionColors;


    private GameObject calculatorCluster;
    private SelectionToolHandler selectionToolHandler;
    private StatusDisplay status;
    private StatusDisplay statusDisplayHUD;
    private StatusDisplay statusDisplayFar;
    private ArrayList data;
    private Thread t;
    private SteamVR_Controller.Device device;
    private int heatmapID = 1;
    private Vector3 heatmapPosition;
    private List<Heatmap> heatmapList = new List<Heatmap>();
    private UnityEngine.Color HighlightMarkerColor;
    private UnityEngine.Color ConfirmMarkerColor;

    void Awake()
    {
        t = null;
        heatmapPosition = heatmapPrefab.transform.position;
        selectionToolHandler = referenceManager.selectionToolHandler;
        status = referenceManager.statusDisplay;
        statusDisplayHUD = referenceManager.statusDisplayHUD;
        statusDisplayFar = referenceManager.statusDisplayFar;
        calculatorCluster = referenceManager.calculatorCluster;
        calculatorCluster.SetActive(false);
        GeneratingHeatmaps = false;
        CellExAlEvents.ConfigLoaded.AddListener(InitColors);
    }

    /// <summary>
    /// Initializes <see cref="expressionColors"/> with the colors in the config file.
    /// </summary>
    private void InitColors()
    {

        int numberOfExpressionColors = CellExAlConfig.NumberOfHeatmapColors;
        expressionColors = new SolidBrush[numberOfExpressionColors];
        UnityEngine.Color low = CellExAlConfig.HeatmapLowExpressionColor;
        UnityEngine.Color mid = CellExAlConfig.HeatmapMidExpressionColor;
        UnityEngine.Color high = CellExAlConfig.HeatmapHighExpressionColor;
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
        HighlightMarkerColor = CellExAlConfig.HeatmapHighlightMarkerColor;
        ConfirmMarkerColor = CellExAlConfig.HeatmapConfirmMarkerColor;
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

    public void CreateHeatmap()
    {
        // name the heatmap "heatmap_X". Where X is some number.
        string heatmapName = "heatmap_" + (selectionToolHandler.fileCreationCtr - 1);
        CellExAlLog.Log("Creating heatmap");
        StartCoroutine(GenerateHeatmapRoutine(heatmapName));
    }

    public Heatmap FindHeatmap(string heatmapName)
    {
        foreach (Heatmap hm in heatmapList)
        {
            if (hm.HeatmapName == heatmapName)
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
            // make a deep copy of the arraylist
            List<GraphPoint> selection = selectionToolHandler.GetLastSelection();
            Dictionary<Cell, int> colors = new Dictionary<Cell, int>();
            foreach (GraphPoint g in selection)
            {
                colors[g.Cell] = g.CurrentGroup;
            }

            // Check if more than one cell is selected
            if (selection.Count < 1)
            {
                CellExAlLog.Log("can not create heatmap with less than 1 graphpoints, aborting");
                yield break;
            }

            int statusId = status.AddStatus("R script generating heatmap");
            int statusIdHUD = statusDisplayHUD.AddStatus("R script generating heatmap");
            int statusIdFar = statusDisplayFar.AddStatus("R script generating heatmap");
            // Start generation of new heatmap in R
            string home = Directory.GetCurrentDirectory();
            int fileCreationCtr = selectionToolHandler.fileCreationCtr - 1;
            string args = home + " " + selectionToolHandler.DataDir + " " + fileCreationCtr + " " + CellExAlUser.UserSpecificFolder;

            string rScriptFilePath = Application.streamingAssetsPath + @"\R\make_heatmap.R";
            string heatmapDirectory = home + @"\Images";
            if (!Directory.Exists(heatmapDirectory))
            {
                CellExAlLog.Log("Creating directory " + CellExAlLog.FixFilePath(heatmapDirectory));
                Directory.CreateDirectory(heatmapDirectory);
            }
            CellExAlLog.Log("Running R script " + CellExAlLog.FixFilePath(rScriptFilePath) + " with the arguments \"" + args + "\"");
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            t = new Thread(() => RScriptRunner.RunFromCmd(rScriptFilePath, args));
            t.Start();
            // Show calculators
            calculatorCluster.SetActive(true);

            while (t.IsAlive)
            {
                yield return null;
            }
            stopwatch.Stop();
            CellExAlLog.Log("Heatmap R script finished in " + stopwatch.Elapsed.ToString());
            status.RemoveStatus(statusId);
            statusDisplayHUD.RemoveStatus(statusIdHUD);
            statusDisplayFar.RemoveStatus(statusIdFar);
            GeneratingHeatmaps = false;
            string newHeatmapFilePath = heatmapDirectory + @"\" + heatmapName + ".png";
            //File.Delete(newHeatmapFilePath);
            //File.Move(heatmapFilePath + @"\heatmap.png", newHeatmapFilePath);

            var heatmap = Instantiate(heatmapPrefab).GetComponent<Heatmap>();
            heatmap.transform.parent = transform;
            heatmap.transform.localPosition = heatmapPosition;
            // save colors before.
            heatmap.SetVars(colors);
            heatmapList.Add(heatmap);

            if (!referenceManager.networkGenerator.GeneratingNetworks)
                calculatorCluster.SetActive(false);

            //heatmap.UpdateImage(newHeatmapFilePath);
            heatmap.GetComponent<AudioSource>().Play();
            heatmap.name = heatmapName;
            heatmap.highlightQuad.GetComponent<Renderer>().material.color = HighlightMarkerColor;
            heatmap.confirmQuad.GetComponent<Renderer>().material.color = ConfirmMarkerColor;
            heatmapID++;
        }
    }
}
