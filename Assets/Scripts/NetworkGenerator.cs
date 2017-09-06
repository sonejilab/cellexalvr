using System.Collections;
using System.IO;
using System.Threading;
using UnityEngine;

/// <summary>
/// This class starts the thread that generates the network files and then tells the inputreader to process them.
/// </summary>
public class NetworkGenerator : MonoBehaviour
{

    public SelectionToolHandler selectionToolHandler;
    public InputReader inputReader;
    public ToggleArcsSubMenu subMenu;
    public StatusDisplay status;
    private Thread t;
    private GenerateNetworksThread gnt;
    public int objectsInSky;
    
    private void Start()
    {
        gnt = new GenerateNetworksThread(selectionToolHandler);
        objectsInSky = 0;
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