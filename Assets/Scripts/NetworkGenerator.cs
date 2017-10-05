using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

/// <summary>
/// This class starts the thread that generates the network files and then tells the inputreader to process them.
/// </summary>
public class NetworkGenerator : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public List<NetworkHandler> networkList = new List<NetworkHandler>();
    public int objectsInSky;

    private SelectionToolHandler selectionToolHandler;
    private InputReader inputReader;
    private StatusDisplay status;
    private Thread t;

    private void Start()
    {
        objectsInSky = 0;
        selectionToolHandler = referenceManager.selectionToolHandler;
        inputReader = referenceManager.inputReader;
        status = referenceManager.statusDisplay;
    }

    /// <summary>
    /// Generates networks based on the selectiontoolhandler's last selection.
    /// </summary>
    public void GenerateNetworks()
    {
        StartCoroutine(GenerateNetworksCoroutine());
    }

    IEnumerator GenerateNetworksCoroutine()
    {
        int statusId = status.AddStatus("R script generating networks");
        // generate the files containing the network information
        string home = Directory.GetCurrentDirectory();
        string args = home + " " + selectionToolHandler.DataDir + " " + (selectionToolHandler.fileCreationCtr - 1) + " " + CellExAlUser.UserSpecificFolder;
        string rScriptFilePath = Application.streamingAssetsPath + @"\R\make_networks.R";
        CellExAlLog.Log("Running R script " + CellExAlLog.FixFilePath(rScriptFilePath) + " with the arguments \"" + args + "\"");
        var stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        t = new Thread(() => RScriptRunner.RunFromCmd(rScriptFilePath, args));
        t.Start();
        while (t.IsAlive)
            yield return null;
        stopwatch.Stop();
        CellExAlLog.Log("Network R script finished in " + stopwatch.Elapsed.ToString());
        status.RemoveStatus(statusId);
        inputReader.ReadNetworkFiles();
    }

    /// <summary>
    /// Finds a networkhandler.
    /// </summary>
    /// <param name="networkName"> The name of the networkhandler </param>
    /// <returns> A reference to the networkhandler, or null if non was found.  </returns>
    public NetworkHandler FindNetworkHandler(string networkName)
    {
        foreach (NetworkHandler nh in networkList)
        {
            if (nh.NetworkHandlerName == networkName)
            {
                return nh;
            }
        }
        return null;
    }
}