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
    public GameObject networkPrefab;
    public NetworkNode networkNodePrefab;
    //public GameObject edgePrefab;
    public GameObject headset;

    private void Start()
    {
        ReadFolder(@"C:\Users\vrproject\Documents\vrJeans\Assets\Data\Bertie2");
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
    /// <returns></returns>
    IEnumerator ReadMDSFiles(string path, string[] mdsFiles, int itemsPerFrame)
    {
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
            UpdateMinMax(newGraph, lines);

            for (int i = 0; i < lines.Length; i += itemsPerFrame)
            {
                for (int j = i; j < i + itemsPerFrame && j < lines.Length; ++j)
                {
                    string line = lines[j];
                    string[] words = line.Split(null);
                    // print(words[0]);
                    graphManager.AddCell(newGraph, words[0], float.Parse(words[1]), float.Parse(words[2]), float.Parse(words[3]));
                }
                yield return new WaitForEndOfFrame();
            }

            /* string[] cellNames = geneLines[0].Split('\t');

            // process each gene and its expression values
            for (int i = 1; i < geneLines.Length; i++)
            {
                string[] words = geneLines[i].Split('\t');
                // the gene name is always the first word on the line
                string geneName = words[0].ToLower();
                float minExpr = 10000f;
                float maxExpr = -1f;
                // find the largest and smallest expression
                for (int j = 1; j < words.Length; j++)
                {
                    float expr = float.Parse(words[j]);
                    if (expr > maxExpr)
                    {
                        maxExpr = expr;
                    }
                    if (expr < minExpr)
                    {
                        minExpr = expr;
                    }
                }
                // figure out how much gene expression each material represents
                float binSize = (maxExpr - minExpr) / 30;
                // for each cell, set its gene expression
                for (int k = 1; k < words.Length; k++)
                {
                    int binIndex = 0;
                    float expr = float.Parse(words[k]);
                    binIndex = (int)((expr - minExpr) / binSize);
                    if (binIndex == 30)
                    {
                        binIndex--;
                    }
                    //cellManager.SetGeneExpression(cellNames[k - 1], geneName, binIndex);
                }
            }*/
            fileIndex++;
        }

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
        loaderController.MoveLoader(new Vector3(0f, -1f, 0f), 6f);
        ReadNetworkFiles();
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
        string[] cntFilePath = Directory.GetFiles(Directory.GetCurrentDirectory() + @"\Assets\Resources\Networks", "*.cnt");
        if (cntFilePath.Length == 0)
        {
            print("more than one .cnt file in network folder");
            return;
        }
        string[] lines = File.ReadAllLines(cntFilePath[0]);
        // read the graph's name and create a skeleton
        string[] firstLine = lines[0].Split(null);
        string graphName = firstLine[firstLine.Length - 1];
        Graph graph = graphManager.FindGraph(graphName);
        GameObject skeleton = graph.CreateConvexHull();

        Dictionary<string, GameObject> networks = new Dictionary<string, GameObject>();
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

            GameObject network = Instantiate(networkPrefab);
            network.transform.parent = skeleton.transform;
            network.transform.localPosition = position;
            network.GetComponent<Renderer>().material.color = color;
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


        }
        // position nodes in a circle
        //Dictionary<GameObject, int> nbrOfNodesAdded = new Dictionary<GameObject, int>(networks.Count);
        //foreach (GameObject network in networks.Values)
        //{
        //    nbrOfNodesAdded[network] = 0;
        //}

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
            nodes[nodeName].transform.localPosition = new Vector3(xcoord / 25f - .5f, ycoord / 25f - .5f, 0f);

        }

        //foreach (NetworkNode node in nodes.Values)
        //{
        //    GameObject parent = node.transform.parent.gameObject;
        //    float networkSize = parent.transform.childCount;
        //    float t = nbrOfNodesAdded[parent];
        //    float x = Mathf.Cos(2f * (float)Math.PI * t / networkSize) * .5f;
        //    float y = Mathf.Sin(2f * (float)Math.PI * t / networkSize) * .5f;
        //    node.transform.localPosition = new Vector3(x, y, 0);
        //    nbrOfNodesAdded[parent]++;
        //}
        //pair together buddies
        //foreach (NetworkNode node in nodes.Values)
        //{
        //    node.PositionBuddies(new Vector3(0, 0, .3f), node.transform.localPosition / 3f);
        //}
        foreach (NetworkNode node in nodes.Values)
        {
            node.AddEdges();
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
