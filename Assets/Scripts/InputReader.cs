using UnityEngine;
using System.Collections;
using System.IO;
using SQLiter;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;
using VRTK;

/// <summary>
/// A class for reading data files and creating GraphPoints at the correct locations 
/// </summary>
public class InputReader : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public NetworkCenter networkPrefab;
    public NetworkNode networkNodePrefab;

    private GraphManager graphManager;
    private CellManager cellManager;
    private LoaderController loaderController;
    private SQLite database;
    private SelectionToolHandler selectionToolHandler;
    private AttributeSubMenu attributeSubMenu;
    private ToggleArcsSubMenu arcsSubMenu;
    private ColorByIndexMenu indexMenu;
    private GameObject headset;
    private StatusDisplay status;
    private SteamVR_TrackedObject rightController;
    private GameManager gameManager;

    [Tooltip("Automatically loads the Bertie dataset")]
    public bool debug = false;

    //Flag for loading previous sessions
    public bool doLoad = false;

    private void Start()
    {
        gameManager = referenceManager.gameManager;
        graphManager = referenceManager.graphManager;
        cellManager = referenceManager.cellManager;
        loaderController = referenceManager.loaderController;
        database = referenceManager.database;
        selectionToolHandler = referenceManager.selectionToolHandler;
        attributeSubMenu = referenceManager.attributeSubMenu;
        arcsSubMenu = referenceManager.arcsSubMenu;
        indexMenu = referenceManager.indexMenu;
        headset = referenceManager.headset;
        status = referenceManager.statusDisplay;
        rightController = referenceManager.rightController;
        if (debug)
        {
            status.gameObject.SetActive(true);
            ReadFolder(Directory.GetCurrentDirectory() + @"\Data\Bertie");
        }

        /*var sceneLoader = GameObject.Find ("Load").GetComponent<Loading> ();
		if (sceneLoader.doLoad) {
			doLoad = true;
			GameObject.Find ("InputFolderList").gameObject.SetActive (false);
			graphManager.LoadDirectory ();
			Debug.Log ("Read Folder: " + graphManager.directory);
			ReadFolder (@graphManager.directory);
		}*/
    }

    /// <summary>
    /// Reads one folder of data and creates the graphs described by the data.
    /// </summary>
    /// <param name="path"> The path to the folder. </param>
    public void ReadFolder(string path)
    {
        gameManager.InformReadFolder(path);
        string workingDirectory = Directory.GetCurrentDirectory();
        string fullPath = workingDirectory + "/Data/" + path;
        database.InitDatabase(fullPath + "\\database.sqlite");
           
        // print(path);
        selectionToolHandler.DataDir = fullPath;

        string runtimegroupsDirectory = workingDirectory + "\\Data\\runtimeGroups";
        if (!Directory.Exists(runtimegroupsDirectory))
        {
            CellExAlLog.Log("Creating directory " + runtimegroupsDirectory);
            Directory.CreateDirectory(runtimegroupsDirectory);
        }

        // clear the runtimeGroups
        string[] txtList = Directory.GetFiles(runtimegroupsDirectory, "*.txt");
        foreach (string f in txtList)
        {
            File.Delete(f);
        }

        if (!debug)
        {
            // clear the network folder
            string networkDirectory = workingDirectory + "\\Resources\\Networks";
            if (!Directory.Exists(networkDirectory))
            {
                CellExAlLog.Log("Creating directory " + networkDirectory);
                Directory.CreateDirectory(networkDirectory);
            }
            string[] networkFilesList = Directory.GetFiles(networkDirectory, "*");
            CellExAlLog.Log("Deleting " + networkFilesList.Length + " files in " + networkDirectory);
            foreach (string f in networkFilesList)
            {
                File.Delete(f);
            }
        }

        string[] mdsFiles = Directory.GetFiles(fullPath, "*.mds");
        StartCoroutine(ReadMDSFiles(fullPath, mdsFiles, 25));

    }

    /// <summary>
    /// Coroutine to create graphs.
    /// </summary>
    /// <param name="path"> The path to the folder where the files are. </param>
    /// <param name="mdsFiles"> The filenames. </param>
    /// <param name="itemsPerFrame"> How many graphpoints should be Instantiated each frame </param>
    IEnumerator ReadMDSFiles(string path, string[] mdsFiles, int itemsPerFrame)
    {
        CellExAlLog.Log("Started reading the data folder at " + path);
        int statusId = status.AddStatus("Reading folder " + path);
        int fileIndex = 0;
        var magnifier = referenceManager.magnifierTool;
        //  Read each .mds file
        // The file format should be
        //  CELLNAME_1 X_COORD Y_COORD Z_COORD
        //  CELLNAME_2 X_COORD Y_COORD Z_COORD
        //  ...
        foreach (string file in mdsFiles)
        {
            Graph newGraph = graphManager.CreateGraph();
            newGraph.GetComponent<GraphInteract>().isGrabbable = false;
            // file will be the full file name e.g C:\...\graph1.mds
            // good programming habits have left us with a nice mix of forward and backward slashes
            string[] regexResult = Regex.Split(file, @"[\\/]");
            string graphFileName = regexResult[regexResult.Length - 1];
            CellExAlLog.Log("Reading graph from " + graphFileName);
            // remove the ".mds" at the end
            newGraph.GraphName = graphFileName.Substring(0, graphFileName.Length - 4);
            newGraph.DirectoryName = regexResult[regexResult.Length - 2];
            // put each line into an array
            string[] lines = File.ReadAllLines(file);
            FileStream fileStream = new FileStream(file, FileMode.Open);
            StreamReader streamReader = new StreamReader(fileStream);
            // we must wait for the graph to fully initialize before adding stuff to it
            while (!newGraph.Ready())
                yield return null;
            newGraph.GetComponent<GraphInteract>().magnifier = magnifier;
            UpdateMinMax(newGraph, lines);

            float maximumDeltaTime = Time.maximumDeltaTime;
            // multiply by 1.1 to allow a loss of ~9.0909% fps
            float maximumDeltaTimeThreshold = maximumDeltaTime * 1.1f;
            int maximumItemsPerFrame = 50;
            while (!streamReader.EndOfStream)
            {
                int itemsThisFrame = 0;
                status.UpdateStatus(statusId, "Reading " + graphFileName + " (" + fileIndex + "/" + mdsFiles.Length + ") " + ((float)fileStream.Position / fileStream.Length) + "%");
                while (!streamReader.EndOfStream)
                {
                    if (itemsThisFrame > maximumItemsPerFrame)
                    {
                        break;
                    }
                    string line = streamReader.ReadLine();
                    string[] words = line.Split(null);
                    graphManager.AddCell(newGraph, words[0], float.Parse(words[1]), float.Parse(words[2]), float.Parse(words[3]));
                    itemsThisFrame++;
                }
                // wait for end of frame
                yield return null;

                // now is the next frame
                float lastFrame = Time.deltaTime;
                if (lastFrame < maximumDeltaTime)
                {
                    // we had some time over last frame
                    maximumItemsPerFrame += 5;
                }
                else if (lastFrame > maximumDeltaTimeThreshold)
                {
                    // we took too much time last frame
                    maximumItemsPerFrame -= 5;
                }
            }
            fileIndex++;
            newGraph.GetComponent<GraphInteract>().isGrabbable = true;
            if (doLoad)
            {
                graphManager.LoadPosition(newGraph, fileIndex);
            }

            CellExAlLog.Log("Successfully read graph from " + graphFileName + " instantating ~" + maximumItemsPerFrame + " graphpoints every frame");
        }
        status.UpdateStatus(statusId, "Reading .meta.cell files");
        // Read the each .meta.cell file
        // The file format should be
        //              TYPE_1  TYPE_2  ...
        //  CELLNAME_1  [0,1]   [0,1]
        //  CELLNAME_2  [0,1]   [0,1]
        // ...
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
        {
            ReadNetworkFiles();
            loaderController.DestroyFolders();
        }
        status.UpdateStatus(statusId, "Reading index.facs file");
        ReadFacsFiles(path);
        status.RemoveStatus(statusId);
    }

    /// <summary>
    /// Reads the index.facs file.
    /// </summary>

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
        /// The file format should be:
        ///             TYPE_1  TYPE_2 ...
        /// CELLNAME_1  VALUE   VALUE  
        /// CELLNAME_2  VALUE   VALUE
        /// ...
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
        CellExAlLog.Log("Successfully read " + fullpath);
    }

    /// <summary>
    /// Helper struct for sorting network keys.
    /// </summary>
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
    /// Helper method to create network nodes.
    /// </summary>
    /// <param name="geneName"> The name of the gene that the network node should represent. </param>
    /// <returns> Returns the newly created NetworkNode. </returns>
    private NetworkNode CreateNetworkNode(string geneName)
    {
        NetworkNode newNode = Instantiate(networkNodePrefab);
        newNode.CameraToLookAt = headset.transform;
        newNode.cellManager = cellManager;
        newNode.Label = geneName;
        newNode.rightController = rightController;
        return newNode;
    }

    /// <summary>
    /// Reads the files containg networks.
    /// </summary>
    public void ReadNetworkFiles()
    {

        // Read the .cnt file
        // The file format should be
        //  X_COORD Y_COORD Z_COORD KEY GRAPHNAME
        //  X_COORD Y_COORD Z_COORD KEY GRAPHNAME
        // ...
        // KEY is simply a hex rgb color code
        // GRAPHNAME is the name of the file (and graph) that the network was made from

        CellExAlLog.Log("Started reading network files");
        // there should only be one .cnt file
        string networkDirectory = Directory.GetCurrentDirectory() + @"\Resources\Networks";
        string[] cntFilePaths = Directory.GetFiles(networkDirectory, "*.cnt");
        string[] nwkFilePath = Directory.GetFiles(networkDirectory, "*.nwk");
        string[] layFilePath = Directory.GetFiles(networkDirectory, "*.lay");

        // make sure there is a .cnt file
        if (cntFilePaths.Length == 0)
        {
            status.ShowStatusForTime("No .cnt file found. This dataset probably does not have a correct database", 10f, Color.red);
            CellExAlLog.Log("ERROR: No .cnt file in network folder " + networkDirectory);
            return;
        }
        if (cntFilePaths.Length > 1)
        {
            CellExAlLog.Log("ERROR: more than one .cnt file in network folder");
            return;
        }

        // make sure there is a .nwk file
        if (nwkFilePath.Length == 0)
        {
            print("no .nwk file in network folder");
            CellExAlLog.Log("ERROR: No .nwk file in network folder " + networkDirectory);
            return;
        }
        FileStream fs = new FileStream(nwkFilePath[0], FileMode.Open);
        StreamReader sr = new StreamReader(fs);
        // 1 MB = 1048576 B
        if (fs.Length > 1048576)
        {
            CellExAlLog.Log("Aborting reading network files because .nwk file is larger than 1 MB",
                            ".nwk file size: " + fs.Length + " B");
            sr.Close();
            fs.Close();
            return;
        }

        // make sure there is a .lay file
        if (layFilePath.Length == 0)
        {
            CellExAlLog.Log("ERROR: No .lay file found in network folder " + networkDirectory);
        }



        string[] lines = File.ReadAllLines(cntFilePaths[0]);
        // read the graph's name and create a skeleton
        string[] firstLine = lines[0].Split(null);
        string graphName = firstLine[firstLine.Length - 1];
        Graph graph = graphManager.FindGraph(graphName);
        GameObject skeleton = graph.CreateConvexHull();
        if (skeleton == null)
        {
            CellExAlLog.Log("ERROR: Could not create convex hull of " + graphName);
            return;
        }
        CellExAlLog.Log("Successfully created convex hull of " + graphName);
        var networkHandler = skeleton.GetComponent<NetworkHandler>();
        networkHandler.NetworkName = "network from " + graph.GraphName;
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
            networkHandler.AddNetwork(network);
            network.Handler = networkHandler;
            graphManager.AddNetwork(networkHandler);
            //network.transform.localPosition -= graph.transform.position;
            foreach (Renderer r in network.GetComponentsInChildren<Renderer>())
            {
                r.material.color = color;
            }
            networks[words[3]] = network;
        }
        CellExAlLog.Log("Successfully read .cnt file");
        // Read the .nwk file
        // The file format should be
        //  PCOR    NODE_1  NODE_2  PVAL    QVAL    PROB    GRPS[I] KEY_1   KEY_2
        //  VALUE   STRING  STRING  VALUE   VALUE   VALUE   HEX_RGB STRING  STRING
        //  VALUE   STRING  STRING  VALUE   VALUE   VALUE   HEX_RGB STRING  STRING
        //  ...
        // We only care about NODE_1, NODE_2, GRPS[I], KEY_1 and KEY_2
        // NODE_1 and NODE_2 are two genenames that should be linked together.
        // GRPS[I] is the network the two genes are in. A gene can be in multiple networks.
        // KEY_1 is the two genenames concatenated together as NODE_1 + NODE_2
        // KEY_2 is the two genenames concatenated together as NODE_2 + NODE_1

        //lines = File.ReadAllLines(nwkFilePath[0]);
        CellExAlLog.Log("Reading .nwk file with " + fs.Length + " bytes");
        Dictionary<string, NetworkNode> nodes = new Dictionary<string, NetworkNode>(1024);
        List<NetworkKeyPair> tmp = new List<NetworkKeyPair>();
        // skip the first line as it is a header
        sr.ReadLine();
        while (!sr.EndOfStream)
        {
            string line = sr.ReadLine();
            if (line == "")
                continue;
            string[] words = line.Split(null);
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
                NetworkNode newNode = CreateNetworkNode(geneName1);
                nodes[node1] = newNode;
                if (networks.ContainsKey(color))
                    nodes[node1].Center = networks[color];
                else
                    print(color);
            }

            if (!nodes.ContainsKey(node2))
            {
                NetworkNode newNode = CreateNetworkNode(geneName2);
                nodes[node2] = newNode;
                nodes[node2].Center = networks[color];
            }

            Transform parentNetwork = networks[words[6]].transform;
            nodes[node1].transform.parent = parentNetwork;
            nodes[node2].transform.parent = parentNetwork;

            // add a bidirectional connection
            nodes[node1].AddNeighbour(nodes[node2]);
            // add the keypair
            tmp.Add(new NetworkKeyPair(color, node1, node2, key1, key2));

        }
        sr.Close();
        fs.Close();
        CellExAlLog.Log("Successfully read .nwk file");
        NetworkKeyPair[] keyPairs = new NetworkKeyPair[tmp.Count];
        tmp.CopyTo(keyPairs);
        // sort the array of keypairs
        // if two keypairs are equal (they both contain the same key), they should be next to each other in the list, otherwise sort based on key1
        Array.Sort(keyPairs, (NetworkKeyPair x, NetworkKeyPair y) => x.key1.Equals(y.key2) ? 0 : x.key1.CompareTo(y.key1));

        // Read the .lay file
        // The file format should be
        // GENENAME X_COORD Y_COORD KEY
        // GENENAME X_COORD Y_COORD KEY
        // ...
        // KEY is the hex rgb color code of the network the gene is in.


        lines = File.ReadAllLines(layFilePath[0]);
        CellExAlLog.Log("Reading .lay file with " + lines.Length + " lines");
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
        CellExAlLog.Log("Successfully read .lay file");
        // since the list is sorted in a smart way, all keypairs that share a key will be next to eachother
        NetworkKeyPair lastKey = new NetworkKeyPair("", "", "", "", "");
        List<NetworkKeyPair> lastNodes = new List<NetworkKeyPair>();
        for (int i = 0; i < keyPairs.Length; ++i)
        {
            NetworkKeyPair keypair = keyPairs[i];
            // if this keypair shares a key with the last keypair
            if (lastKey.key1 == keypair.key1 || lastKey.key1 == keypair.key2)
            {
                // add arcs to all previous pairs that also shared a key
                foreach (NetworkKeyPair node in lastNodes)
                {
                    var center = nodes[node.node1].Center;
                    center.AddArc(nodes[node.node1], nodes[node.node2], nodes[keypair.node1], nodes[keypair.node2]);
                }
            }
            else
            {
                // clear the list if this key did not match the last one
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

        List<int> arcsCombinedList = new List<int>();
        // toggle the arcs off
        foreach (NetworkCenter network in networks.Values)
        {
            var arcscombined = network.CreateCombinedArcs();
            arcsCombinedList.Add(arcscombined);
            network.SetArcsVisible(false);
            network.SetCombinedArcsVisible(false);
        }
        var max = 0;
        foreach (int i in arcsCombinedList)
        {
            if (max < i)
                max = i;
        }
        foreach (NetworkCenter network in networks.Values)
        {
            network.ColorCombinedArcs(max);
        }
        CellExAlLog.Log("Successfully created " + j + " networks with a total of " + nodes.Values.Count + " nodes");
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
