using UnityEngine;
using System.Collections;
using System.IO;
using SQLiter;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System;
using TMPro;
using System.Threading;

/// <summary>
/// A class for reading data files and creating objects in the virtual environment.
/// 
/// </summary>
public class InputReader : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public NetworkCenter networkPrefab;

    public TextMeshPro graphName;

    public GameObject lineprefab;

    private GraphManager graphManager;
    private CellManager cellManager;
    private LoaderController loaderController;
    private SQLite database;
    private SelectionToolHandler selectionToolHandler;
    private FlashGenesMenu flashGenesMenu;
    private AttributeSubMenu attributeSubMenu;
    private ToggleArcsSubMenu arcsSubMenu;
    private ColorByIndexMenu indexMenu;
    private GameObject headset;
    private StatusDisplay status;
    private StatusDisplay statusDisplayHUD;
    private StatusDisplay statusDisplayFar;
    private GameManager gameManager;
    private NetworkGenerator networkGenerator;

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
        flashGenesMenu = referenceManager.flashGenesMenu;
        attributeSubMenu = referenceManager.attributeSubMenu;
        arcsSubMenu = referenceManager.arcsSubMenu;
        indexMenu = referenceManager.indexMenu;
        headset = referenceManager.headset;
        status = referenceManager.statusDisplay;
        statusDisplayHUD = referenceManager.statusDisplayHUD;
        statusDisplayFar = referenceManager.statusDisplayFar;
        networkGenerator = referenceManager.networkGenerator;

        if (debug)
        {
            //status.gameObject.SetActive(true);
            ReadFolder(@"Mouse_HSPC");
        }
        CellexalUser.UsernameChanged.AddListener(LoadPreviousGroupings);
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
        string workingDirectory = Directory.GetCurrentDirectory();
        string fullPath;
        if (SceneManagerHelper.ActiveSceneName == "TutorialScene")
        {
            fullPath = workingDirectory + "\\TutorialData\\" + path;
        }
        else
        {
            fullPath = workingDirectory + "\\Data\\" + path;
        }
        CellexalLog.Log("Started reading the data folder at " + CellexalLog.FixFilePath(fullPath));
        CellexalUser.DataSourceFolder = path;
        //LoadPreviousGroupings();
        database.InitDatabase(fullPath + "\\database.sqlite");
        if (Directory.Exists(workingDirectory + "\\Output"))
        {
            if (File.Exists(workingDirectory + "\\Output\\r_log.txt"))
            {
                CellexalLog.Log("Deleting old r log file");
                File.Delete(workingDirectory + "\\Output\\r_log.txt");
            }
        }
        // print(path);
        selectionToolHandler.DataDir = fullPath;
        if (!debug)
        {
            // clear the network folder
            string networkDirectory = workingDirectory + "\\Resources\\Networks";
            if (!Directory.Exists(networkDirectory))
            {
                CellexalLog.Log("Creating directory " + CellexalLog.FixFilePath(networkDirectory));
                Directory.CreateDirectory(networkDirectory);
            }
            string[] networkFilesList = Directory.GetFiles(networkDirectory, "*");
            CellexalLog.Log("Deleting " + networkFilesList.Length + " files in " + CellexalLog.FixFilePath(networkDirectory));
            foreach (string f in networkFilesList)
            {
                File.Delete(f);
            }
        }

        string[] mdsFiles = Directory.GetFiles(fullPath, "*.mds");
        if (mdsFiles.Length == 0)
        {
            CellexalError.SpawnError("Empty dataset", "The loaded dataset did not contain any .mds files. Make sure you have placed the dataset files in the correct folder.");
        }
        CellexalLog.Log("Reading " + mdsFiles.Length + " .mds files");
        StartCoroutine(ReadMDSFiles(fullPath, mdsFiles));
    }

    /// <summary>
    /// Coroutine to create graphs.
    /// </summary>
    /// <param name="path"> The path to the folder where the files are. </param>
    /// <param name="mdsFiles"> The filenames. </param>
    IEnumerator ReadMDSFiles(string path, string[] mdsFiles)
    {
        int statusId = status.AddStatus("Reading folder " + path);
        int statusIdHUD = statusDisplayHUD.AddStatus("Reading folder " + path);
        int statusIdFar = statusDisplayFar.AddStatus("Reading folder " + path);
        int fileIndex = 0;
        var magnifier = referenceManager.magnifierTool;
        //  Read each .mds file
        // The file format should be
        //  CELLNAME_1 X_COORD Y_COORD Z_COORD
        //  CELLNAME_2 X_COORD Y_COORD Z_COORD
        //  ...

        float maximumDeltaTime = 0.05f; // 20 fps
        int maximumItemsPerFrame = CellexalConfig.GraphLoadingCellsPerFrameStartCount;
        int itemsThisFrame = 0;
        int totalNbrOfCells = 0;
        foreach (string file in mdsFiles)
        {
            Graph newGraph = graphManager.CreateGraph();
            newGraph.GetComponent<GraphInteract>().isGrabbable = false;
            // file will be the full file name e.g C:\...\graph1.mds
            // good programming habits have left us with a nice mix of forward and backward slashes
            string[] regexResult = Regex.Split(file, @"[\\/]");
            string graphFileName = regexResult[regexResult.Length - 1];
            CellexalLog.Log("Reading graph from " + graphFileName);
            // remove the ".mds" at the end
            newGraph.GraphName = graphFileName.Substring(0, graphFileName.Length - 4);
            //var textmeshgraphname = Instantiate(graphName);
            //textmeshgraphname.transform.position = newGraph.transform.position;
            //textmeshgraphname.transform.Translate(0f, 0.6f, 0f);
            //textmeshgraphname.text = newGraph.GraphName;
            //textmeshgraphname.transform.LookAt(referenceManager.headset.transform.position);
            //textmeshgraphname.transform.Rotate(0f, 180f, 0f);
            newGraph.DirectoryName = regexResult[regexResult.Length - 2];

            //FileStream mdsFileStream = new FileStream(file, FileMode.Open);
            using (StreamReader mdsStreamReader = new StreamReader(file))
            {
                List<string> cellnames = new List<string>();
                List<float> xcoords = new List<float>();
                List<float> ycoords = new List<float>();
                List<float> zcoords = new List<float>();

                while (!mdsStreamReader.EndOfStream)
                {
                    string[] words = mdsStreamReader.ReadLine().Split(null);
                    if (words.Length != 4)
                    {
                        continue;
                    }
                    cellnames.Add(words[0]);
                    xcoords.Add(float.Parse(words[1]));
                    ycoords.Add(float.Parse(words[2]));
                    zcoords.Add(float.Parse(words[3]));
                }
                if (fileIndex == 0)
                {
                    totalNbrOfCells = xcoords.Count;
                }
                // we must wait for the graph to fully initialize before adding stuff to it
                while (!newGraph.Ready())
                    yield return null;
                newGraph.GetComponent<GraphInteract>().magnifier = magnifier;
                newGraph.GetComponent<GraphInteract>().referenceManager = referenceManager;
                UpdateMinMax(newGraph, xcoords, ycoords, zcoords);

                for (int i = 0; i < xcoords.Count; i += itemsThisFrame)
                {
                    itemsThisFrame = 0;
                    status.UpdateStatus(statusId, "Reading " + graphFileName + " (" + fileIndex + "/" + mdsFiles.Length + ") " + ((float)mdsStreamReader.BaseStream.Position / mdsStreamReader.BaseStream.Length) + "%");
                    statusDisplayHUD.UpdateStatus(statusIdHUD, "Reading " + graphFileName + " (" + fileIndex + "/" + mdsFiles.Length + ") " + ((float)mdsStreamReader.BaseStream.Position / mdsStreamReader.BaseStream.Length) + "%");
                    statusDisplayFar.UpdateStatus(statusIdFar, "Reading " + graphFileName + " (" + fileIndex + "/" + mdsFiles.Length + ") " + ((float)mdsStreamReader.BaseStream.Position / mdsStreamReader.BaseStream.Length) + "%");
                    //print(maximumItemsPerFrame);
                    for (int j = i; j < (i + maximumItemsPerFrame) && j < xcoords.Count; ++j)
                    {
                        graphManager.AddCell(newGraph, cellnames[j], xcoords[j], ycoords[j], zcoords[j]);
                        itemsThisFrame++;
                    }
                    // wait for end of frame
                    yield return null;

                    // now is the next frame
                    float lastFrame = Time.deltaTime;
                    if (lastFrame < maximumDeltaTime)
                    {
                        // we had some time over last frame
                        maximumItemsPerFrame += CellexalConfig.GraphLoadingCellsPerFrameIncrement;
                    }
                    else if (lastFrame > maximumDeltaTime && maximumItemsPerFrame > CellexalConfig.GraphLoadingCellsPerFrameIncrement * 2)
                    {
                        // we took too much time last frame
                        maximumItemsPerFrame -= CellexalConfig.GraphLoadingCellsPerFrameIncrement;
                    }
                    //UnityEditor.EditorApplication.isPlaying = false;
                }
                fileIndex++;
                // tell the graph that the info text is ready to be set
                newGraph.SetInfoText();
                newGraph.GetComponent<GraphInteract>().isGrabbable = true;
                System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();
                newGraph.CreateColliders();
                stopwatch.Stop();
                CellexalLog.Log("Created " + newGraph.GetComponents<BoxCollider>().Length + " colliders in " + stopwatch.Elapsed.ToString() + " for graph " + graphFileName);
                if (doLoad)
                {
                    graphManager.LoadPosition(newGraph, fileIndex);
                }
                //mdsFileStream.Close();
                mdsStreamReader.Close();
                // if (debug)
                //     newGraph.CreateConvexHull();
            }
            CellexalLog.Log("Successfully read graph from " + graphFileName + " instantiating ~" + maximumItemsPerFrame + " graphpoints every frame");
        }

        ReadAttributeFiles(path);
        ReadBooleanExpressionFiles(path);

        loaderController.loaderMovedDown = true;
        loaderController.MoveLoader(new Vector3(0f, -2f, 0f), 8f);
        if (debug)
        {
            ReadNetworkFiles(0);
            loaderController.DestroyFolders();
        }
        status.UpdateStatus(statusId, "Reading index.facs file");
        statusDisplayHUD.UpdateStatus(statusIdHUD, "Reading index.facs file");
        statusDisplayFar.UpdateStatus(statusIdFar, "Reading index.facs file");
        ReadFacsFiles(path, totalNbrOfCells);
        flashGenesMenu.CreateTabs(path);
        status.RemoveStatus(statusId);
        statusDisplayHUD.RemoveStatus(statusIdHUD);
        statusDisplayFar.RemoveStatus(statusIdFar);
        CellexalEvents.GraphsLoaded.Invoke();

        StartCoroutine(InitialCheckCoroutine());
        if (debug)
        {
            //  yield return new WaitForSeconds(3);
            //  database.QueryGene("gata1", DrawSomeLines);
            //var expr = BooleanExpression.ParseFile(Directory.GetCurrentDirectory() + @"\Data\" + CellexalUser.DataSourceFolder + "\\expr2.ott");
            //print(expr.ToString());
            //cellManager.ColorByAttributeExpression(expr);
            //    cellManager.SaveFlashGenesData(ReadFlashingGenesFiles("Data/Bertie/flashing_genes_cell_cycle.fgv"));
        }

    }

    public void ReadAttributeFiles(string path)
    {
        // Read the each .meta.cell file
        // The file format should be
        //              TYPE_1  TYPE_2  ...
        //  CELLNAME_1  [0,1]   [0,1]
        //  CELLNAME_2  [0,1]   [0,1]
        // ...
        string[] metacellfiles = Directory.GetFiles(path, "*.meta.cell");
        foreach (string metacellfile in metacellfiles)
        {
            FileStream metacellFileStream = new FileStream(metacellfile, FileMode.Open);
            StreamReader metacellStreamReader = new StreamReader(metacellFileStream);

            // first line is a header line
            string header = metacellStreamReader.ReadLine();
            string[] attributeTypes = header.Split(null);
            string[] actualAttributeTypes = new string[attributeTypes.Length - 1];
            for (int i = 1; i < attributeTypes.Length; ++i)
            {
                //if (attributeTypes[i].Length > 10)
                //{
                //    attributeTypes[i] = attributeTypes[i].Substring(0, 10);
                //}
                actualAttributeTypes[i - 1] = attributeTypes[i];
                //print(attributeTypes[i]);
            }
            while (!metacellStreamReader.EndOfStream)
            {
                string line = metacellStreamReader.ReadLine();
                if (line == "")
                    continue;

                string[] words = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                string cellname = words[0];
                for (int j = 1; j < words.Length; ++j)
                {
                    if (words[j] == "1")
                        cellManager.AddAttribute(cellname, attributeTypes[j], (j - 1) % CellexalConfig.AttributeColors.Length);
                }
            }
            metacellStreamReader.Close();
            metacellFileStream.Close();
            attributeSubMenu.CreateAttributeButtons(actualAttributeTypes);
            cellManager.Attributes = actualAttributeTypes;
        }
    }

    public void ReadBooleanExpressionFiles(string path)
    {
        string[] files = Directory.GetFiles(path, "*.ott");
        List<Tuple<string, BooleanExpression.Expr>> expressions = new List<Tuple<string, BooleanExpression.Expr>>(files.Length);
        foreach (string file in files)
        {
            // TODO CELLEXAL: add aliases support and stuff to these files
            expressions.Add(new Tuple<string, BooleanExpression.Expr>(file, BooleanExpression.ParseFile(file)));
        }

        attributeSubMenu.AddExpressionButtons(expressions.ToArray());
    }

    // public void DrawSomeLines(SQLite database)
    // {
    //     StartCoroutine(DrawSomeLinesCoroutine(database));
    // }

    // public IEnumerator DrawSomeLinesCoroutine(SQLite database)
    // {
    //     var line = Instantiate(lineprefab);
    //     Vector3[] pos = new Vector3[database._result.Count];
    //     Tuple<string, float>[] exprs = new Tuple<string, float>[database._result.Count];
    //     for (int i = 0; i < database._result.Count; ++i)
    //     {
    //         exprs[i] = (Tuple<string, float>)(database._result[i]);
    //     }
    //
    //     Array.Sort(exprs, (Tuple<string, float> x, Tuple<string, float> y) => (y.Item2.CompareTo(x.Item2)));
    //
    //
    //     var lineRenderer = line.GetComponent<LineRenderer>();
    //     lineRenderer.startColor = Color.red;
    //     lineRenderer.endColor = Color.blue;
    //     for (int i = 0; i < exprs.Length; ++i)
    //     {
    //
    //         lineRenderer.positionCount = i + 1;
    //         lineRenderer.SetPosition(i, graphManager.FindGraphPoint("DDRTree", exprs[i].Item1).transform.position);
    //     }
    //
    //     yield return null;
    // }

    private IEnumerator InitialCheckCoroutine()
    {
        string dataSourceFolder = Directory.GetCurrentDirectory() + @"\Data\" + CellexalUser.DataSourceFolder;
        string userFolder = CellexalUser.UserSpecificFolder;
        string args = dataSourceFolder + " " + userFolder;
        string rScriptFilePath = Application.streamingAssetsPath + @"\R\initial_check.R";
        CellexalLog.Log("Running initial check script at " + rScriptFilePath + " with the arguments " + args);
        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
        stopwatch.Start();
        Thread t = new Thread(() => RScriptRunner.RunFromCmd(rScriptFilePath, args));
        t.Start();
        while (t.IsAlive)
        {
            yield return null;
        }
        stopwatch.Stop();
        CellexalLog.Log("Updating R Object finished in " + stopwatch.Elapsed.ToString());
        LoadPreviousGroupings();
    }

    /// <summary>
    /// Reads a file containing lists of genes that should be flashed.
    /// </summary>
    /// <param name="path"> The path to the file. </param>
    /// <returns> An array of categories. Each has its name at index zero, and the rest of each array is filled with the content of the category. </returns>
    public string[][] ReadFlashingGenesFiles(string path)
    {
        FileStream fileStream = new FileStream(path, FileMode.Open);
        StreamReader streamReader = new StreamReader(fileStream);
        // The file format should be
        // CATEGORY_1, CATEGORY 2  ...
        // GENE_11 ,   GENE_21
        // GENE_12 ,   GENE_22
        // ...
        // All categories and genes should be comma seperated.

        string header = streamReader.ReadLine();
        string[] words = header.Split(',');
        List<string>[] genes = new List<string>[words.Length];
        for (int i = 0; i < words.Length; ++i)
        {
            // put the gene category names at the first index of each list.
            genes[i] = new List<string>();
            genes[i].Add(words[i].Trim());
        }

        while (!streamReader.EndOfStream)
        {
            string line = streamReader.ReadLine();
            if (line.Length == 0)
                continue;
            words = line.Split(',');
            for (int j = 0; j < words.Length; ++j)
            {
                string gene = words[j].Trim();
                if (gene != string.Empty)
                {
                    genes[j].Add(gene.ToLower());
                }
            }
        }

        string[][] result = new string[genes.Length][];
        for (int i = 0; i < words.Length; ++i)
        {
            result[i] = genes[i].ToArray();
        }

        streamReader.Close();
        fileStream.Close();
        return result;
    }

    /// <summary>
    /// Reads the index.facs file.
    /// </summary>
    private void ReadFacsFiles(string path, int nbrOfCells)
    {
        string fullpath = path + "\\index.facs";

        if (!File.Exists(fullpath))
        {
            print("File " + fullpath + " not found");
            CellexalLog.Log(".facs file not found");
            return;
        }

        FileStream fileStream = new FileStream(fullpath, FileMode.Open);
        StreamReader streamReader = new StreamReader(fileStream);

        // The file format should be:
        //             TYPE_1  TYPE_2 ...
        // CELLNAME_1  VALUE   VALUE  
        // CELLNAME_2  VALUE   VALUE
        // ...

        string headerline = streamReader.ReadLine();
        string[] header = headerline.Split(new string[] { "\t", " " }, StringSplitOptions.RemoveEmptyEntries);
        float[] min = new float[header.Length];
        float[] max = new float[header.Length];
        int i = 0;
        for (; i < min.Length; ++i)
        {
            min[i] = float.MaxValue;
            max[i] = float.MinValue;
        }
        string[] cellnames = new string[nbrOfCells];
        float[,] values = new float[nbrOfCells, header.Length];

        // read the file, calculate the min and max values and save all values
        for (i = 0; !streamReader.EndOfStream; ++i)
        {
            string[] line = streamReader.ReadLine().Split(null);
            for (int j = 0; j < line.Length - 1; ++j)
            {
                float value = float.Parse(line[j + 1]);
                cellnames[i] = line[0];
                values[i, j] = value;
                if (value < min[j])
                    min[j] = value;
                if (value > max[j])
                    max[j] = value;
            }
        }
        // now that we know the min and max values we can iterate over the values once again
        for (i = 0; i < values.GetLength(0); ++i)
        {
            for (int j = 0; j < values.GetLength(1); ++j)
            {
                // normalize to the range [0, 29]
                float colorIndexFloat = ((values[i, j] - min[j]) / (max[j] - min[j])) * (CellexalConfig.NumberOfExpressionColors - 1);
                int colorIndex = Mathf.FloorToInt(colorIndexFloat);
                cellManager.AddFacs(cellnames[i], header[j], colorIndex);
            }
        }
        streamReader.Close();
        fileStream.Close();
        indexMenu.CreateButtons(header);
        cellManager.Facs = header;
        CellexalLog.Log("Successfully read " + CellexalLog.FixFilePath(fullpath));
    }

    /// <summary>
    /// Helper struct for sorting network keys.
    /// </summary>
    public struct NetworkKeyPair
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
    public void ReadNetworkFiles(int layoutSeed)
    {
        StartCoroutine(ReadNetworkFilesCoroutine(layoutSeed));
    }
    private IEnumerator ReadNetworkFilesCoroutine(int layoutSeed)
    {
        CellexalLog.Log("Started reading network files");
        string networkDirectory = Directory.GetCurrentDirectory() + @"\Resources\Networks";
        if (!Directory.Exists(networkDirectory))
        {
            CellexalError.SpawnError("Error when generating networks", string.Format("No network directory found at {0}, make sure the network generating r script has executed properly.", CellexalLog.FixFilePath(networkDirectory)));
            yield break;
        }
        string[] cntFilePaths = Directory.GetFiles(networkDirectory, "*.cnt");
        string[] nwkFilePaths = Directory.GetFiles(networkDirectory, "*.nwk");

        // make sure there is a .cnt file
        if (cntFilePaths.Length == 0)
        {
            status.ShowStatusForTime("No .cnt file found. This dataset probably does not have a correct database", 10f, Color.red);
            statusDisplayHUD.ShowStatusForTime("No .cnt file found. This dataset probably does not have a correct database", 10f, Color.red);
            statusDisplayFar.ShowStatusForTime("No .cnt file found. This dataset probably does not have a correct database", 10f, Color.red);
            CellexalError.SpawnError("Error when generating networks", string.Format("No .cnt file found at {0}, make sure the network generating r script has executed properly by checking the r_log.txt in the output folder.", CellexalLog.FixFilePath(networkDirectory)));
            yield break;
        }

        if (cntFilePaths.Length > 1)
        {
            CellexalError.SpawnError("Error when generating networks", string.Format("More than one .cnt file found at {0}, make sure the network generating r script has executed properly by checking the r_log.txt in the output folder.", CellexalLog.FixFilePath(networkDirectory)));
            yield break;
        }

        FileStream cntFileStream = new FileStream(cntFilePaths[0], FileMode.Open);
        StreamReader cntStreamReader = new StreamReader(cntFileStream);

        // make sure there is a .nwk file
        if (nwkFilePaths.Length == 0)
        {
            CellexalError.SpawnError("Error when generating networks", string.Format("No .nwk file found at {0}, make sure the network generating r script has executed properly by checking the r_log.txt in the output folder.", CellexalLog.FixFilePath(networkDirectory)));
            yield break;
        }
        FileStream nwkFileStream = new FileStream(nwkFilePaths[0], FileMode.Open);
        StreamReader nwkStreamReader = new StreamReader(nwkFileStream);
        // 1 MB = 1048576 B
        if (nwkFileStream.Length > 1048576)
        {
            CellexalError.SpawnError("Error when generating networks", string.Format(".nwk file is larger than 1 MB. .nwk file size: {0} B", nwkFileStream.Length));
            nwkStreamReader.Close();
            nwkFileStream.Close();
            yield break;
        }


        // Read the .cnt file
        // The file format should be
        //  X_COORD Y_COORD Z_COORD KEY GRAPHNAME
        //  X_COORD Y_COORD Z_COORD KEY GRAPHNAME
        // ...
        // KEY is simply a hex rgb color code
        // GRAPHNAME is the name of the file (and graph) that the network was made from

        // read the graph's name and create a skeleton
        bool firstLine = true;
        Dictionary<string, NetworkCenter> networks = new Dictionary<string, NetworkCenter>();
        // these variables are set when the first line is read
        Graph graph = null;
        GameObject skeleton = null;
        NetworkHandler networkHandler = null;
        string networkHandlerName = null;

        Dictionary<string, float> maxNegPcor = new Dictionary<string, float>();
        Dictionary<string, float> minNegPcor = new Dictionary<string, float>();
        Dictionary<string, float> maxPosPcor = new Dictionary<string, float>();
        Dictionary<string, float> minPosPcor = new Dictionary<string, float>();

        while (!cntStreamReader.EndOfStream)
        {
            string line = cntStreamReader.ReadLine();
            if (line == "")
                continue;
            string[] words = line.Split(null);

            if (firstLine)
            {
                firstLine = false;
                string graphName = words[words.Length - 1];
                graph = graphManager.FindGraph(graphName);
                if (graph == null)
                {
                    CellexalError.SpawnError("Error when generating networks", string.Format("Could not find the graph named {0} when trying to create a convex hull, make sire there is a .mds and .hull file with the same name in the dataset.", graphName));
                    yield break;
                }
                skeleton = graph.CreateConvexHull();
                if (skeleton == null)
                {
                    CellexalError.SpawnError("Error when generating networks", string.Format("Could not create a convex hull for the graph named {0}, this could be because the convex hull file is incorrect", graphName));
                    yield break;
                }
                CellexalLog.Log("Successfully created convex hull of " + graphName);
                networkHandler = skeleton.GetComponent<NetworkHandler>();
                networkHandlerName = "NetworkHandler_" + graphName + "-" + (selectionToolHandler.fileCreationCtr + 1);
                networkHandler.name = networkHandlerName;
            }
            float x = float.Parse(words[0]);
            float y = float.Parse(words[1]);
            float z = float.Parse(words[2]);
            // the color is a hex string e.g. #FF0099
            Color color = new Color();
            string colorString = words[3];
            ColorUtility.TryParseHtmlString(colorString, out color);

            maxPosPcor[colorString] = 0f;
            minPosPcor[colorString] = float.MaxValue;
            maxNegPcor[colorString] = float.MinValue;
            minNegPcor[colorString] = 0f;

            Vector3 position = graph.ScaleCoordinates(x, y, z);
            NetworkCenter network = networkGenerator.CreateNetworkCenter(networkHandler, colorString, position, layoutSeed);
            //network.transform.localPosition -= graph.transform.position;
            foreach (Renderer r in network.GetComponentsInChildren<Renderer>())
            {
                if (r.gameObject.GetComponent<CellexalButton>() == null)
                {
                    r.material.color = color;
                }
            }
            networks[colorString] = network;
        }

        CellexalLog.Log("Successfully read .cnt file");

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

        CellexalLog.Log("Reading .nwk file with " + nwkFileStream.Length + " bytes");
        Dictionary<string, NetworkNode> nodes = new Dictionary<string, NetworkNode>(1024);
        List<NetworkKeyPair> tmp = new List<NetworkKeyPair>();
        // skip the first line as it is a header
        nwkStreamReader.ReadLine();

        while (!nwkStreamReader.EndOfStream)
        {
            string line = nwkStreamReader.ReadLine();
            if (line == "")
                continue;
            if (line[0] == '#')
                continue;
            string[] words = line.Split(null);
            string color = words[6];
            string geneName1 = words[1];
            string node1 = geneName1 + color;
            string geneName2 = words[2];
            string node2 = geneName2 + color;
            string key1 = words[7];
            string key2 = words[8];
            float pcor = float.Parse(words[0]);
            if (pcor < 0)
            {
                if (pcor < minNegPcor[color])
                    minNegPcor[color] = pcor;
                if (pcor > maxNegPcor[color])
                    maxNegPcor[color] = pcor;
            }
            else
            {
                if (pcor < minPosPcor[color])
                    minPosPcor[color] = pcor;
                if (pcor > maxPosPcor[color])
                    maxPosPcor[color] = pcor;
            }
            // add the nodes if they don't already exist
            if (!nodes.ContainsKey(node1))
            {
                NetworkNode newNode = networkGenerator.CreateNetworkNode(geneName1, networks[color]);
                nodes[node1] = newNode;
            }

            if (!nodes.ContainsKey(node2))
            {
                NetworkNode newNode = networkGenerator.CreateNetworkNode(geneName2, networks[color]);
                nodes[node2] = newNode;
            }

            Transform parentNetwork = networks[words[6]].transform;
            nodes[node1].transform.parent = parentNetwork;
            nodes[node2].transform.parent = parentNetwork;

            // add a bidirectional connection
            nodes[node1].AddNeighbour(nodes[node2], pcor);
            // add the keypair
            tmp.Add(new NetworkKeyPair(color, node1, node2, key1, key2));

        }
        nwkStreamReader.Close();
        nwkFileStream.Close();
        CellexalLog.Log("Successfully read .nwk file");
        NetworkKeyPair[] keyPairs = new NetworkKeyPair[tmp.Count];
        tmp.CopyTo(keyPairs);
        // sort the array of keypairs
        // if two keypairs are equal (they both contain the same key), they should be next to each other in the list, otherwise sort based on key1
        Array.Sort(keyPairs, (NetworkKeyPair x, NetworkKeyPair y) => x.key1.Equals(y.key2) ? 0 : x.key1.CompareTo(y.key1));

        //yield return null;
        networkHandler.CalculateLayoutOnAllNetworks();

        // wait for all networks to finish their layout
        while (networkHandler.layoutApplied != networks.Count)
            yield return null;

        foreach (var network in networks)
        {
            network.Value.MaxPosPcor = maxPosPcor[network.Key];
            network.Value.MinPosPcor = minPosPcor[network.Key];
            network.Value.MaxNegPcor = maxNegPcor[network.Key];
            network.Value.MinNegPcor = minNegPcor[network.Key];
        }

        foreach (var node in nodes.Values)
        {
            node.ColorEdges();
        }
        yield return null;
        // give all nodes in the networks edges
        networkHandler.CreateArcs(ref keyPairs, ref nodes);


        cntStreamReader.Close();
        cntFileStream.Close();
        nwkStreamReader.Close();
        nwkFileStream.Close();
        CellexalLog.Log("Successfully created " + networks.Count + " networks with a total of " + nodes.Values.Count + " nodes");
        networkHandler.CreateNetworkAnimation(networkHandler.transform.position, graph.transform.position);
    }


    /// <summary>
    /// Determines the maximum and the minimum values of the dataset.
    /// Will be used for the scaling part onto the graphArea.
    ///</summary>
    void UpdateMinMax(Graph graph, List<float> xcoords, List<float> ycoords, List<float> zcoords)
    {
        Vector3 maxCoordValues = new Vector3();
        maxCoordValues.x = maxCoordValues.y = maxCoordValues.z = float.MinValue;
        Vector3 minCoordValues = new Vector3();
        minCoordValues.x = minCoordValues.y = minCoordValues.z = float.MaxValue;
        for (int i = 0; i < xcoords.Count; ++i)
        {
            // the coordinates are split with whitespace characters
            if (xcoords[i] < minCoordValues.x)
                minCoordValues.x = xcoords[i];
            if (xcoords[i] > maxCoordValues.x)
                maxCoordValues.x = xcoords[i];

            if (ycoords[i] < minCoordValues.y)
                minCoordValues.y = ycoords[i];
            if (ycoords[i] > maxCoordValues.y)
                maxCoordValues.y = ycoords[i];

            if (zcoords[i] < minCoordValues.z)
                minCoordValues.z = zcoords[i];
            if (zcoords[i] > maxCoordValues.z)
                maxCoordValues.z = zcoords[i];

        }
        graph.SetMinMaxCoords(minCoordValues, maxCoordValues);
    }

    /// <summary>
    /// Read all the user.group files which cointains the grouping information from previous sessions.
    /// </summary>
    public void LoadPreviousGroupings()
    {
        string dataFolder = CellexalUser.UserSpecificFolder;
        string groupingsInfoFile = dataFolder + "\\groupings_info.txt";
        CellexalLog.Log("Started reading the previous groupings files");
        //print(groupingsInfoFile);
        if (!File.Exists(groupingsInfoFile))
        {
            CellexalLog.Log("WARNING: No groupings info file found at " + CellexalLog.FixFilePath(groupingsInfoFile));
            return;
        }
        FileStream fileStream = new FileStream(groupingsInfoFile, FileMode.Open);
        StreamReader streamReader = new StreamReader(fileStream);
        // skip the header
        streamReader.ReadLine();
        List<string> groupingNames = new List<string>();
        List<int> fileLengths = new List<int>();
        string line = "";
        string[] words = null;
        while (!streamReader.EndOfStream)
        {
            line = streamReader.ReadLine();
            if (line == "") continue;
            words = line.Split(new char[] { '\t', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            // set the grouping's name to [the grouping's number]\n[number of colors in grouping]\n[number of cells in groupings]
            string groupingName = words[0];
            int indexOfLastDot = groupingName.LastIndexOf(".");
            if (indexOfLastDot == -1)
            {
                CellexalLog.Log("WARNING: Could not find \'.\' in \"" + words[0] + "\"");
                indexOfLastDot = groupingName.Length - 1;
            }
            //string groupingNumber = groupingName.Substring(indexOfLastDot, groupingName.Length - indexOfLastDot);
            groupingNames.Add(groupingName + "\n" + words[1] + "\n" + words[2]);
            fileLengths.Add(int.Parse(words[2]));
        }
        streamReader.Close();
        fileStream.Close();

        CellexalLog.Log("Reading " + groupingNames.Count + " files");
        // initialize the arrays
        string[][] cellNames = new string[groupingNames.Count][];
        int[][] groups = new int[groupingNames.Count][];
        string[] graphNames = new string[groupingNames.Count];
        Dictionary<int, Color>[] groupingColors = new Dictionary<int, Color>[groupingNames.Count];
        for (int i = 0; i < cellNames.Length; ++i)
        {
            cellNames[i] = new string[fileLengths[i]];
            groups[i] = new int[fileLengths[i]];
        }
        for (int i = 0; i < groupingNames.Count; ++i)
        {
            groupingColors[i] = new Dictionary<int, Color>();
        }
        words = null;
        string[] files = Directory.GetFiles(dataFolder, "*.cgr");
        for (int i = 0; i < fileLengths.Count; ++i)
        {
            string file = files[i];
            fileStream = new FileStream(file, FileMode.Open);
            streamReader = new StreamReader(fileStream);

            for (int j = 0; j < fileLengths[i]; ++j)
            {
                line = streamReader.ReadLine();

                words = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                cellNames[i][j] = words[0];

                try
                {
                    int group = int.Parse(words[3]);
                    groups[i][j] = group;
                    Color groupColor;
                    ColorUtility.TryParseHtmlString(words[1], out groupColor);
                    groupingColors[i][group] = groupColor;
                }
                catch (FormatException e)
                {
                    foreach (string s in words)
                    {
                        print(s);
                    }
                    print(words[3] + " " + file + " " + j + "\n" + e.StackTrace);
                    streamReader.Close();
                    fileStream.Close();
                    return;
                }

            }
            graphNames[i] = words[2];
            streamReader.Close();
            fileStream.Close();
        }
        // someone please rename this
        referenceManager.createSelectionFromPreviousSelectionMenu.CreateSelectionFromPreviousSelectionButtons(graphNames, groupingNames.ToArray(), cellNames, groups, groupingColors);
        CellexalLog.Log("Successfully read " + groupingNames.Count + " files");
    }

    /// <summary>
    /// Sorts a list of gene expressions based on the mean expression level.
    /// </summary>
    /// <param name="genes"> An array of genes to be sorted. This array will be reordered but the elements won't be modified. </param>
    /// <param name="expressions"> An array of arrays containing the expressions.</param>
    /// <remarks> The expressions parameter should be set up as follows: the first dimension should be the genes and the second the cells, i.e. looking for a certain expression is done via <code>expressions[geneIndex][cellIndex]</code></remarks>
    public void SortGenesMeanExpression(ref string[] genes, ref int[][] expressions)
    {
        StringFloatPair[] pairList = new StringFloatPair[genes.Length];

        for (int i = 0; i < expressions.Length; ++i)
        {
            float meanExpressions = 0;
            for (int j = 0; j < expressions[i].Length; ++j)
            {
                meanExpressions += expressions[i][j];
            }
            pairList[i] = new StringFloatPair(genes[i], meanExpressions + expressions[i].Length);
        }
        // sort based on mean expressions, will also sort the expression matrix accordingly
        Array.Sort(pairList, expressions);
        for (int i = 0; i < pairList.Length; ++i)
        {
            genes[i] = pairList[i].s;
        }
    }

    /// <summary>
    /// Helper class to sort a list of genes based on mean gene expression.
    /// </summary>
    private class StringFloatPair : IComparable<StringFloatPair>
    {
        public string s;
        public float f;

        public StringFloatPair(string s, float f)
        {
            this.s = s;
            this.f = f;
        }

        public int CompareTo(StringFloatPair other)
        {
            return f.CompareTo(other.f);
        }
    }
}
