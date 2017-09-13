using System;
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

    public SelectionToolHandler selectionToolHandler;
    public GameObject heatmapPrefab;
    public ErrorMessageController errorMessageController;
    public GraphManager graphManager;
    public GameObject fire;
    public StatusDisplay status;
    private ArrayList data;
    private Thread t;
    private bool running = false;
    private SteamVR_Controller.Device device;
    private GameObject hourglass;
    private GameObject heatBoard;
    private int heatmapID = 1;
    private Vector3 heatmapPosition;
    private List<Heatmap> heatmapList = new List<Heatmap>();

    void Start()
    {
        t = null;
        hourglass = GameObject.Find("WaitingForHeatboardHourglass");
        hourglass.SetActive(false);
        heatmapPosition = heatmapPrefab.transform.position;
    }

    internal void DeleteHeatmaps()
    {
        foreach (Heatmap h in heatmapList)
        {
            Destroy(h.gameObject);
        }
        heatmapList.Clear();
    }

    public void CreateHeatmap()
    {
        StartCoroutine(GenerateHeatmapRoutine());
    }

    /// <summary>
    /// Coroutine for creating a heatmap.
    /// </summary>
    IEnumerator GenerateHeatmapRoutine()
    {
        if (selectionToolHandler.selectionConfirmed && !selectionToolHandler.GetHeatmapCreated())
        {
            // make a deep copy of the arraylist
            ArrayList selection = selectionToolHandler.GetLastSelection();
            Dictionary<Cell, Color> colors = new Dictionary<Cell, Color>();
            foreach (GraphPoint g in selection)
            {
                colors[g.Cell] = g.GetComponent<Renderer>().material.color;
            }

            // Check if more than one color is selected
            if (selection.Count < 2)
            {
                yield break;
            }
            Color c1 = ((GraphPoint)selection[0]).GetComponent<Renderer>().material.color;
            bool colorFound = false;
            for (int i = 1; i < selection.Count; ++i)
            {
                Color c2 = ((GraphPoint)selection[i]).GetComponent<Renderer>().material.color;
                if (!((c1.r == c2.r) && (c1.g == c2.g) && (c1.b == c2.b)))
                {
                    colorFound = true;
                    break;
                }
            }
            if (!colorFound)
            {
                // Generate error message if less than two colors are selected
                errorMessageController.DisplayErrorMessage(3);
                yield break;
            }

            int statusId = status.AddStatus("R script generating heatmap");
            // Start generation of new heatmap in R
            string home = Directory.GetCurrentDirectory();
            int fileCreationCtr = selectionToolHandler.fileCreationCtr - 1;
            string args = home + " " + selectionToolHandler.DataDir + " " + fileCreationCtr;
            t = new Thread(() => RScriptRunner.RunFromCmd(@"\Assets\Scripts\R\make_heatmap.R", args));
            t.Start();
            running = true;
            // Show hourglass
            hourglass.SetActive(true);

            while (t.IsAlive)
            {
                yield return null;
            }
            status.RemoveStatus(statusId);
            running = false;
            string heatmapFilePath = home + @"\Assets\Images";
            // rename the file from heatmap.png to heatmap_X.png. Where X is some number.
            string newHeatmapFilePath = heatmapFilePath + @"\heatmap_" + fileCreationCtr + ".png";
            //File.Delete(newHeatmapFilePath);
            //File.Move(heatmapFilePath + @"\heatmap.png", newHeatmapFilePath);

            heatBoard = Instantiate(heatmapPrefab);
            heatBoard.transform.parent = transform;
            heatBoard.transform.localPosition = heatmapPosition;
            Heatmap heatmap = heatBoard.GetComponent<Heatmap>();
            // TODO: fix recoloring not working when generating multiple heatmaps at once
            // save colors before.
            heatmap.SetVars(graphManager, selectionToolHandler, colors, fire);
            heatmapList.Add(heatmap);

            hourglass.SetActive(false);

            heatmap.UpdateImage(newHeatmapFilePath);
            heatBoard.GetComponent<AudioSource>().Play();

            heatmapID++;

        }
    }

}
