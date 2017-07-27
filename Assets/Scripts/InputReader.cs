using UnityEngine;
using System.Collections;
using System.IO;
using SQLiter;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;

/// <summary>
/// A class for reading data files and creating GraphPoints at the correct locations 
/// </summary>
public class InputReader : MonoBehaviour
{

    public GraphManager graphManager;
    public CellManager cellManager;
    public LoaderController loaderController;
    public SQLite database;
    public SelectionToolHandler selectionToolHandler;
    public AttributeSubMenu attributeSubMenu;
    public ToggleArcsSubMenu arcsSubMenu;
    public ColorByIndexMenu indexMenu;
    public NetworkCenter networkPrefab;
    public NetworkNode networkNodePrefab;
    public GameObject headset;
    public StatusDisplay status;
    [Tooltip("Automatically loads a the Bertie dataset")]
    public bool debug = false;

    private void Start()
    {
        if (debug)
        {
            ReadFolder(@"C:\Users\vrproject\Documents\vrJeans\Assets\Data\Bertie");
        }
    }

    /// <summary>
    /// Reads one folder of data and creates the graphs described by the data.
    /// </summary>
    /// <param name="path"> The path to the folder. </param>
    public void ReadFolder(string path)
    {

        database.InitDatabase(path + "\\database.sqlite");
        // print(path);
        selectionToolHandler.DataDir = path;
        // clear the runtimeGroups
        string[] txtList = Directory.GetFiles(Directory.GetCurrentDirectory() + "\\Assets\\Data\\runtimeGroups", "*.txt");
        // print(Directory.GetCurrentDirectory() + "\\Assets\\Data\\runtimeGroups");
        foreach (string f in txtList)
        {
            File.Delete(f);
        }

        //string[] geneexprFiles = Directory.GetFiles(path, "*.expr");

        //if (geneexprFiles.Length != 1)
        //{
        //    throw new System.InvalidOperationException("There must be exactly one gene expression data file");
        //}

        string[] mdsFiles = Directory.GetFiles(path, "*.mds");
        //StartCoroutine(ReadFiles(path, 25));
        StartCoroutine(ReadMDSFiles(path, mdsFiles, 25));

    }

    /// <summary>
    /// Coroutine to create graphs.
    /// </summary>
    /// <param name="path"> The path to the folder where the files are. </param>
    /// <param name="mdsFiles"> The filenames. </param>
    /// <param name="itemsPerFrame"> How many graphpoints should be Instantiated each frame </param>
    IEnumerator ReadMDSFiles(string path, string[] mdsFiles, int itemsPerFrame)
    {
        int statusId = status.AddStatus("Reading folder " + path);
        int fileIndex = 0;
        foreach (string file in mdsFiles)
        {
            Graph newGraph = graphManager.CreateGraph();
            //graphManager.SetActiveGraph(fileIndex);
            // file will be the full file name e.g C:\...\graph1.mds
            // good programming habits have left us with a nice mix of forward and backward slashes
            string[] regexResult = Regex.Split(file, @"[\\/]");
            string graphFileName = regexResult[regexResult.Length - 1];
            // remove the ".mds" at the end
            newGraph.GraphName = graphFileName.Substring(0, graphFileName.Length - 4);
            newGraph.DirectoryName = regexResult[regexResult.Length - 2];
            // put each line into an array
            string[] lines = File.ReadAllLines(file);
            //string[] geneLines = System.IO.File.ReadAllLines(geneexprFilename);
            // we must wait for the graph to fully initialize before adding stuff to it
            while (!newGraph.Ready())
                yield return null;

            UpdateMinMax(newGraph, lines);

            for (int i = 0; i < lines.Length; i += itemsPerFrame)
            {
                status.UpdateStatus(statusId, "Reading " + graphFileName + ". " + i + "/" + lines.Length);
                for (int j = i; j < i + itemsPerFrame && j < lines.Length; ++j)
                {
                    string line = lines[j];
                    string[] words = line.Split(null);
                    // print(words[0]);
                    graphManager.AddCell(newGraph, words[0], float.Parse(words[1]), float.Parse(words[2]), float.Parse(words[3]));
                }
                yield return null;
            }
            fileIndex++;
        }
        status.UpdateStatus(statusId, "Reading .meta.cell files");
        string[] metacellfiles = Directory.GetFiles(path, "*.meta.cell");
        foreach (string metacellfile in metacellfiles)
        {
            // print(metacellfile);
            string[] lines = File.ReadAllLines(metacellfile);
            // first line is a header line
            string header = lines[0];
            string[] attributeTypes = header.Split(null);
            string[] actualAttributeTypes = new string[attributeTypes.Length - 1];
            for (int i = 1; i < attributeTypes.Length; ++i)
            {
                if (attributeTypes[i].Length > 10)
                {
                    attributeTypes[i] = attributeTypes[i].Substring(0, 10);
                }
                actualAttributeTypes[i - 1] = attributeTypes[i];
                //print(attributeTypes[i]);
            }
            for (int i = 1; i < lines.Length; ++i)
            {
                string[] line = lines[i].Split(null);
                string cellname = line[0];
                for (int j = 1; j < line.Length; ++j)
                {
                    cellManager.AddAttribute(cellname, attributeTypes[j], line[j]);
                }
            }
            attributeSubMenu.CreateAttributeButtons(actualAttributeTypes);
        }

        loaderController.loaderMovedDown = true;
        loaderController.MoveLoader(new Vector3(0f, -2f, 0f), 8f);
        if (debug)
            ReadNetworkFiles();
        status.UpdateStatus(statusId, "Reading index.facs file");
        ReadFacsFiles(path);
        status.RemoveStatus(statusId);
    }

    private void ReadFacsFiles(string path)
    {
        string fullpath = path + "/index.facs";

        if (!File.Exists(fullpath))
        {
            print("File " + fullpath + " not found");
            return;
        }

        string[] lines = File.ReadAllLines(fullpath);
        if (lines.Length == 0)
        {
            // file is empty
            return;
        }

        string headerline = lines[0];
        string[] header = headerline.Split(null);
        float[] min = new float[header.Length - 1];
        float[] max = new float[header.Length - 1];
        for (int i = 0; i < min.Length; ++i)
        {
            min[i] = float.MaxValue;
            max[i] = float.MinValue;
        }
        // calculate the minimum and mean values for each column
        for (int i = 1; i < lines.Length; ++i)
        {
            string[] line = lines[i].Split(null);
            for (int j = 0; j < line.Length - 1; ++j)
            {
                float value = float.Parse(line[j + 1]);
                if (value < min[j])
                    min[j] = value;
                if (value > max[j])
                    max[j] = value;
            }
        }

        for (int i = 1; i < lines.Length; ++i)
        {
            string[] line = lines[i].Split(null);
            string cellName = line[0];
            for (int j = 1; j < line.Length; ++j)
            {
                // normalize to the range [0, 29]
                float colorIndexFloat = ((float.Parse(line[j]) - min[j - 1]) / (max[j - 1] - min[j - 1])) * 29f;
                int colorIndex = Mathf.FloorToInt(colorIndexFloat);
                //print(colorIndex);
                cellManager.AddFacs(line[0], header[j], colorIndex);
            }
        }

        string[] names = new string[header.Length - 1];
        for (int i = 0; i < header.Length - 1; ++i)
        {
            names[i] = header[i + 1];
        }
        indexMenu.CreateColorByIndexButtons(names);
    }


    private struct NetworkKeyPair
    {
        public string color, node1, node2, key1, key2;

        public NetworkKeyPair(string c, string n1, string n2, string k1, string k2)
        {
            color = c;
            key1 = k1;
            key2 = k2;
            node1 = n1;
            node2 = n2;
        }
    }

    /// <summary>
    /// Reads the files containg networks.
    /// </summary>
    public void ReadNetworkFiles()
    {

        // read the .cnt file
        //
        // it should contain information about the average positions of the networks

        // there should only be one .cnt file
        string[] cntFilePaths = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\Assets\Resources\Networks", "*.cnt");
        if (cntFilePaths.Length == 0)
        {
            print("no .cnt file found");
            return;
        }
        if (cntFilePaths.Length > 1)
        {
            print("more than one .cnt file in network folder");
            return;
        }
        string[] lines = File.ReadAllLines(cntFilePaths[0]);
        // read the graph's name and create a skeleton
        string[] firstLine = lines[0].Split(null);
        string graphName = firstLine[firstLine.Length - 1];
        Graph graph = graphManager.FindGraph(graphName);
        GameObject skeleton = graph.CreateConvexHull();
        if (skeleton == null) return;

        Dictionary<string, NetworkCenter> networks = new Dictionary<string, NetworkCenter>();
        foreach (string line in lines)
        {
            if (line == "")
                continue;
            // print(line);
            string[] words = line.Split(null);
            // print(words[0]);
            float x = float.Parse(words[0]);
            float y = float.Parse(words[1]);
            float z = float.Parse(words[2]);
            // the color is a hex string e.g. #FF0099
            Color color = new Color();
            ColorUtility.TryParseHtmlString(words[3], out color);
            Vector3 position = graph.ScaleCoordinates(x, y, z);

            NetworkCenter network = Instantiate(networkPrefab);
            network.transform.parent = skeleton.transform;
            network.transform.localPosition = position;
            //network.transform.localPosition -= graph.transform.position;
            foreach (Renderer r in network.GetComponentsInChildren<Renderer>())
            {
                r.material.color = color;
            }
            networks[words[3]] = network;
        }

        // read the .nwk file
        //
        // it should contain all information about each individual node of some networks

        // there should only be one .nwk file
        string[] nwkFilePath = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\Assets\Resources\Networks", "*.nwk");
        if (nwkFilePath.Length == 0)
        {
            print("more than one .nwk file in network folder");
            return;
        }
        lines = File.ReadAllLines(nwkFilePath[0]);
        Dictionary<string, NetworkNode> nodes = new Dictionary<string, NetworkNode>(1000);
        List<NetworkKeyPair> tmp = new List<NetworkKeyPair>();
        // skip the first line as it is a header
        for (int i = 1; i < lines.Length; ++i)
        {
            if (lines[i] == "")
                continue;
            string[] words = lines[i].Split(null);
            string color = words[6];
            string geneName1 = words[1];
            string node1 = geneName1 + color;
            string geneName2 = words[2];
            string node2 = geneName2 + color;
            string key1 = words[7];
            string key2 = words[8];
            // add the nodes if they don't already exist
            if (!nodes.ContainsKey(node1))
            {
                NetworkNode newNode = Instantiate(networkNodePrefab);
                newNode.CameraToLookAt = headset.transform;
                newNode.Label = geneName1;
                nodes[node1] = newNode;
            }

            if (!nodes.ContainsKey(node2))
            {
                NetworkNode newNode = Instantiate(networkNodePrefab);
                newNode.CameraToLookAt = headset.transform;
                newNode.Label = geneName2;
                nodes[node2] = newNode;
            }

            Transform parentNetwork = networks[words[6]].transform;
            nodes[node1].transform.parent = parentNetwork;
            nodes[node2].transform.parent = parentNetwork;

            // add a bidirectional connection
            nodes[node1].AddNeighbour(nodes[node2]);
            // add the keypair
            tmp.Add(new NetworkKeyPair(color, node1, node2, key1, key2));

        }

        NetworkKeyPair[] keyPairs = new NetworkKeyPair[tmp.Count];
        tmp.CopyTo(keyPairs);
        // sort the array of keypairs
        // if two keypairs are equal (they both contain the same key), they should be next to each other in the list, otherwise sort based on key1
        Array.Sort(keyPairs, (NetworkKeyPair x, NetworkKeyPair y) => x.key1.Equals(y.key2) ? 0 : x.key1.CompareTo(y.key1));

        string[] layFilePath = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\Assets\Resources\Networks", "*.lay");
        lines = File.ReadAllLines(layFilePath[0]);


        foreach (string line in lines)
        {
            if (line == "")
                continue;
            string[] words = line.Split(null);
            string geneName = words[0];
            float xcoord = float.Parse(words[1]);
            float ycoord = float.Parse(words[2]);
            string color = words[3];
            string nodeName = geneName + color;
            nodes[nodeName].transform.localPosition = new Vector3(xcoord / 2f, ycoord / 2f, 0f);

        }

        NetworkKeyPair lastKey = new NetworkKeyPair("", "", "", "", "");
        List<NetworkKeyPair> lastNodes = new List<NetworkKeyPair>();
        for (int i = 0; i < keyPairs.Length; ++i)
        {
            NetworkKeyPair keypair = keyPairs[i];
            if (lastKey.key1 == keypair.key1 || lastKey.key1 == keypair.key2)
            {
                foreach (NetworkKeyPair node in lastNodes)
                {
                    nodes[node.node1].AddArc(nodes[node.node2], nodes[keypair.node1], nodes[keypair.node2]);
                }
            }
            else
            {
                lastNodes.Clear();
            }
            lastNodes.Add(keypair);
            lastKey = keypair;

        }

        foreach (NetworkNode node in nodes.Values)
        {
            node.AddEdges();
        }

        // copy the networks to an array
        NetworkCenter[] networkCenterArray = new NetworkCenter[networks.Count];
        int j = 0;
        foreach (NetworkCenter n in networks.Values)
        {
            networkCenterArray[j++] = n;
        }
        //return networkCenterArray;
        arcsSubMenu.CreateToggleArcsButtons(networkCenterArray);

        // toggle the arcs off
        foreach (NetworkCenter network in networks.Values)
        {
            network.SetArcsVisible(false);
        }

    }


    /// <summary>
    /// Determines the maximum and the minimum values of the dataset.
    /// Will be used for the scaling part onto the graphArea.
    ///</summary>

    void UpdateMinMax(Graph graph, string[] lines)
    {
        Vector3 maxCoordValues = new Vector3();
        maxCoordValues.x = maxCoordValues.y = maxCoordValues.z = float.MinValue;
        Vector3 minCoordValues = new Vector3();
        minCoordValues.x = minCoordValues.y = minCoordValues.z = float.MaxValue;
        foreach (string line in lines)
        {
            // the coordinates are split with whitespace characters
            string[] words = line.Split(null);
            float[] coords = new float[3];
            coords[0] = float.Parse(words[1]);
            coords[1] = float.Parse(words[2]);
            coords[2] = float.Parse(words[3]);
            if (coords[0] < minCoordValues.x)
                minCoordValues.x = coords[0];
            if (coords[0] > maxCoordValues.x)
                maxCoordValues.x = coords[0];
            if (coords[1] < minCoordValues.y)
                minCoordValues.y = coords[1];
            if (coords[1] > maxCoordValues.y)
                maxCoordValues.y = coords[1];
            if (coords[2] < minCoordValues.z)
                minCoordValues.z = coords[2];
            if (coords[2] > maxCoordValues.z)
                maxCoordValues.z = coords[2];

        }
        graphManager.SetMinMaxCoords(graph, minCoordValues, maxCoordValues);
    }
}
