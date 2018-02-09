using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

/// <summary>
/// A generator for heatmaps.
/// </summary>
public class HeatmapGenerator : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public GameObject heatmapPrefab;
    public ErrorMessageController errorMessageController;

    public bool GeneratingHeatmaps { get; private set; }

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

    void Start()
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

            heatmap.UpdateImage(newHeatmapFilePath);
            heatmap.GetComponent<AudioSource>().Play();
            heatmap.name = heatmapName;
            heatmapID++;
        }
    }
}
