using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;

/// <summary>
/// This class starts the thread that generates the network files and then tells the inputreader to process them.
/// </summary>
public class NetworkGenerator : MonoBehaviour
{
    public ReferenceManager referenceManager;

    private SelectionToolHandler selectionToolHandler;
    private InputReader inputReader;
    private ToggleArcsSubMenu subMenu;
    private StatusDisplay status;
    private Thread t;
    public int objectsInSky;

    private void Start()
    {
        objectsInSky = 0;
        selectionToolHandler = referenceManager.selectionToolHandler;
        inputReader = referenceManager.inputReader;
        subMenu = referenceManager.arcsSubMenu;
        status = referenceManager.statusDisplay;
    }

    public void GenerateNetworks()
    {
        StartCoroutine(GenerateNetworksCoroutine());
    }

    IEnumerator GenerateNetworksCoroutine()
    {
        int statusId = status.AddStatus("R script generating networks");
        // generate the files containing the network information
        string home = Directory.GetCurrentDirectory();
        string args = home + " " + selectionToolHandler.DataDir + " " + (selectionToolHandler.fileCreationCtr - 1);
        t = new Thread(() => RScriptRunner.RunFromCmd(@"\Assets\Scripts\R\make_networks.R", args));
        t.Start();
        while (t.IsAlive)
            yield return null;
        status.RemoveStatus(statusId);
        inputReader.ReadNetworkFiles();
    }
}