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
    public NetworkCenter networkCenterPrefab;
    public NetworkNode networkNodePrefab;
    public Material networkNodeDefaultMaterial;
    public Material networkLineDefaultMaterial;
    public List<NetworkHandler> networkList = new List<NetworkHandler>();
    public int objectsInSky;

    public bool GeneratingNetworks { get; private set; }

    public Material[] LineMaterials;

    private GameObject calculatorCluster;
    private SelectionToolHandler selectionToolHandler;
    private InputReader inputReader;
    private GraphManager graphManager;
    private GameObject headset;
    private StatusDisplay status;
    private StatusDisplay statusDisplayHUD;
    private StatusDisplay statusDisplayFar;
    private Thread t;

    public NetworkGenerator()
    {
        CellExAlEvents.ConfigLoaded.AddListener(CreateLineMaterials);
    }

    private void Start()
    {
        objectsInSky = 0;
        selectionToolHandler = referenceManager.selectionToolHandler;
        inputReader = referenceManager.inputReader;
        graphManager = referenceManager.graphManager;
        headset = referenceManager.headset;
        status = referenceManager.statusDisplay;
        statusDisplayHUD = referenceManager.statusDisplayHUD;
        statusDisplayFar = referenceManager.statusDisplayFar;
        calculatorCluster = referenceManager.calculatorCluster;
    }

    /// <summary>
    /// Creates the materials used when drawing the lines between genes in networks.
    /// </summary>
    private void CreateLineMaterials()
    {
        int numColors = CellExAlConfig.NumberOfNetworkLineColors;
        if (numColors < 1)
        {
            CellExAlLog.Log("WARNING: NumberOfNetworkLineColors in config file must be atleast 1");
            numColors = 1;
        }
        List<Material> result = new List<Material>();
        // Create a cuboid in a 3D color spectrum and choose the colors
        // from the spectrum at (sort of) evenly distributed points in that cuboid.
        float spaceBetween = 1f / numColors;
        int sidex = numColors;
        int sidey = numColors / 6;
        int sidez = numColors / 6;
        if (sidey < 1)
        {
            // if numcolors is too low the other for loops don't work because sidey = 0
            for (int cubex = 0; cubex < sidex; ++cubex)
            {
                Material newMaterial = new Material(networkLineDefaultMaterial);
                newMaterial.color = Color.HSVToRGB(1f - cubex * spaceBetween, 1f, 1f);
                result.Add(newMaterial);
            }
        }
        else
        {
            for (int cubex = 0; cubex < sidex; ++cubex)
            {
                for (int cubey = 0; cubey < sidey; ++cubey)
                {
                    for (int cubez = 0; cubez < sidez; ++cubez)
                    {
                        Material newMaterial = new Material(networkLineDefaultMaterial);
                        newMaterial.color = Color.HSVToRGB(1f - cubex * spaceBetween, 1f - cubey * spaceBetween * 6, 1f - cubez * spaceBetween * 6);
                        result.Add(newMaterial);
                    }
                }
            }
        }
        LineMaterials = result.ToArray();
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
        GeneratingNetworks = true;
        calculatorCluster.SetActive(true);
        int statusId = status.AddStatus("R script generating networks");
        int statusIdHUD = statusDisplayHUD.AddStatus("R script generating networks");
        int statusIdFar = statusDisplayFar.AddStatus("R script generating networks");
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
        GeneratingNetworks = false;
        if (!referenceManager.heatmapGenerator.GeneratingHeatmaps)
            calculatorCluster.SetActive(false);
        //statusDisplayHUD.RemoveStatus(statusIdHUD);
        statusDisplayFar.RemoveStatus(statusIdFar);
        inputReader.ReadNetworkFiles();
    }

    /// <summary>
    /// Helper method to create network nodes.
    /// </summary>
    /// <param name="geneName"> The name of the gene that the network node should represent. </param>
    /// <returns> Returns the newly created NetworkNode. </returns>
    public NetworkNode CreateNetworkNode(string geneName, NetworkCenter center)
    {
        NetworkNode newNode = Instantiate(networkNodePrefab);
        newNode.GetComponent<Renderer>().sharedMaterial = networkNodeDefaultMaterial;
        newNode.CameraToLookAt = headset.transform;
        newNode.SetReferenceManager(referenceManager);
        newNode.Label = geneName;
        newNode.Center = center;
        return newNode;
    }

    /// <summary>
    /// Creates a new network center.
    /// </summary>
    /// <param name="handler"> The handler the center should be connected to. </param>
    /// <param name="name"> The name of the center. </param>
    /// <param name="position"> The position it should sit at. Should be from <see cref="Graph.ScaleCoordinates(float, float, float)"/>. </param>
    /// <returns> The new network center. </returns>
    public NetworkCenter CreateNetworkCenter(NetworkHandler handler, string name, Vector3 position)
    {
        NetworkCenter network = Instantiate(networkCenterPrefab);
        network.transform.parent = handler.gameObject.transform;
        network.transform.localPosition = position;
        handler.AddNetwork(network);
        network.Handler = handler;
        network.NetworkCenterName = handler.NetworkHandlerName + name;
        graphManager.AddNetwork(handler);
        networkList.Add(handler);
        return network;
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