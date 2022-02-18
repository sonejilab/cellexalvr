using UnityEngine;
using System.Collections;
using System.IO;
using System.Linq;
using SQLiter;
using System.Collections.Generic;
using System;
using System.Threading;
using CellexalVR.Menu.SubMenus;
using CellexalVR.General;
using CellexalVR.AnalysisObjects;
using CellexalVR.DesktopUI;
using CellexalVR.Extensions;
using System.Drawing;
using System.Diagnostics;
using CellexalVR.AnalysisLogic.H5reader;
using CellexalVR.PDFViewer;
using UnityEngine.XR;
using AnalysisLogic;
using DefaultNamespace;
using System.Text.RegularExpressions;
using UnityEngine.VFX;
using CellexalVR.Spatial;

namespace CellexalVR.AnalysisLogic
{
    /// <summary>
    /// A class for reading data files and creating objects in the virtual environment.
    /// 
    /// </summary>
    public class InputReader : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject spatialGraphPrefab;
        public int facsGraphCounter;
        public bool attributeFileRead;
        public AttributeReader attributeReader;
        public MDSReader mdsReader;
        public PointCloudGenerator pointCloudGenerator;

        private readonly char[] separators = { ' ', '\t', ',' };
        private CellManager cellManager;
        private SQLite database;
        private SelectionManager selectionManager;
        private ColorByIndexMenu indexMenu;
        private GraphFromMarkersMenu createFromMarkerMenu;
        private NetworkReader networkReader;
        private PDFMesh pdfMesh;

        private GameObject headset;

        //private StatusDisplay status;
        //private StatusDisplay statusDisplayHUD;
        //private StatusDisplay statusDisplayFar;
        private GraphGenerator graphGenerator;
        private string currentPath;
        private Bitmap image1;
        public Dictionary<string, H5Reader> h5readers = new Dictionary<string, H5Reader>();

        [Tooltip("Automatically loads the Bertie dataset")]
        public bool debug;
        //Flag for loading previous sessions

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            XRSettings.eyeTextureResolutionScale = 1.5f;
            // QualitySettings.vSyncCount = 0;
            h5readers = new Dictionary<string, H5Reader>();
            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            cellManager = referenceManager.cellManager;
            database = referenceManager.database;
            selectionManager = referenceManager.selectionManager;
            indexMenu = referenceManager.indexMenu;
            createFromMarkerMenu = referenceManager.createFromMarkerMenu;
            if (CrossSceneInformation.Spectator)
            {
                headset = referenceManager.spectatorRig;
                referenceManager.headset = headset;
                headset.SetActive(true);
            }
            else
            {
                headset = referenceManager.headset;
                // referenceManager.spectatorRig.SetActive(false);
            }

            graphGenerator = referenceManager.graphGenerator;
            pdfMesh = referenceManager.pdfMesh;
            currentPath = "";
            facsGraphCounter = 0;
            RScriptRunner.SetReferenceManager(referenceManager);
            // CellexalEvents.UsernameChanged.AddListener(LoadPreviousGroupings);
        }


        [ConsoleCommand("inputReader", folder: "Data", aliases: new string[] { "readfolder", "rf" })]
        public void ReadFolderConsole(string path)
        {
            referenceManager.multiuserMessageSender.SendMessageReadFolder(path);
            ReadFolder(path);
        }

        [ConsoleCommand("inputReader", folder: "Data", aliases: new string[] { "readbigfolder", "rbf" })]
        public void ReadBigFolderConsole(string path)
        {
            referenceManager.multiuserMessageSender.SendMessageReadFolder(path);
            StartCoroutine(ReadBigFolder(path));
            // ReadFolder(path);
        }

        [ConsoleCommand("inputReader", folder: "Data", aliases: new string[] { "readscarffolder", "rscf" })]
        public void ReadScarfFolderConsole(string path)
        {
            //referenceManager.multiuserMessageSender.SendMessageReadScarfFolder(path);
            ScarfManager.instance.LoadPreprocessedData(path);
        }

        [ConsoleCommand("inputReader", folder: "Data", aliases: new string[] { "readprevioussession", "rps" })]
        public void ReadPreviousSessionConsole(string path, string fromPreviousSession = "")
        {
            // referenceManager.multiuserMessageSender.SendMessageReadFolder(path);
            ReadFolder(path, null, fromPreviousSession);
        }

        /// <summary>
        /// Reads one folder of data and creates the graphs described by the data.
        /// </summary>
        /// 
        /// <param name="path">path to the file</param>
        private void ReadFileH5(string path, Dictionary<string, string> h5config)
        {
            bool confExists = Directory.EnumerateFiles("Data\\" + path, "*.conf").Any();
            if (!confExists)
            {
                if (h5config == null)
                {
                    referenceManager.h5ReaderAnnotatorScriptManager.AddAnnotator(path);
                    return;
                }
            }

            string fullPath = Directory.GetCurrentDirectory() + "\\Data\\" + path;
            GameObject go = new GameObject(path);
            H5Reader h5Reader = go.AddComponent<H5Reader>();
            h5readers.Add(path, h5Reader);
            h5Reader.SetConf(path, h5config);
            StartCoroutine(h5Reader.H5ReadGraphs(fullPath));
            if (PhotonNetwork.isMasterClient)
            {
                referenceManager.configManager.MultiUserSynchronise();
            }
        }

        private IEnumerator ReadBigFolder(string path)
        {
            if (!referenceManager.loaderController.loaderMovedDown)
            {
                referenceManager.loaderController.loaderMovedDown = true;
                referenceManager.loaderController.MoveLoader(new Vector3(0f, -2f, 0f), 2f);
            }


            Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.InvariantCulture;
            string workingDirectory = Directory.GetCurrentDirectory();
            string fullPath = workingDirectory + "\\Data\\" + path;
            CellexalUser.DataSourceFolder = path;
            database.InitDatabase(fullPath + "\\database.sqlite");

            string[] files = Directory.GetFiles(fullPath, "*.mds");
            pointCloudGenerator.mdsFileCount = files.Length;
            // string mdsFile = files[0];
            foreach (string mdsFile in files)
            {
                bool spatial = mdsFile.Contains("spatial");
                PointCloud pc = pointCloudGenerator.CreateNewPointCloud(spatial);
                string[] regexResult = Regex.Split(mdsFile, @"[\\/]");
                string graphFileName = regexResult[regexResult.Length - 1];
                //pc.gameObject.name = graphFileName.Substring(0, graphFileName.Length - 4);
                pc.GraphName = graphFileName.Substring(0, graphFileName.Length - 4);
                pc.originalName = pc.GraphName;
                float x;
                float y;
                float z;
                using (StreamReader streamReader = new StreamReader(mdsFile))
                {
                    streamReader.ReadLine();

                    int i = 0;
                    while (!streamReader.EndOfStream)
                    {
                        if (i % 10000 == 0) yield return null;
                        string[] words = streamReader.ReadLine().Split(separators);
                        string cellName;
                        // reading 2d graph or img pixel coordinates for spatial slice.
                        if (words.Length == 2)
                        {
                            cellName = i.ToString();
                            x = (float.Parse(words[0]));
                            y = (float.Parse(words[1]));
                            z = 0f;
                        }
                        else
                        {
                            cellName = words[0];
                            x = (float.Parse(words[1]));
                            y = (float.Parse(words[2]));
                            z = float.Parse(words[3]);
                        }
                        pointCloudGenerator.AddGraphPoint(cellName, x, y, z);
                        int textureX = i % PointCloudGenerator.textureWidth;
                        int textureY = (i / PointCloudGenerator.textureWidth);
                        TextureHandler.instance.textureCoordDict[words[0]] = new Vector2Int(textureX, textureY);
                        i++;
                    }
                }
                pointCloudGenerator.SpawnPoints(pc);
                GC.Collect();
            }

            if (files.Length > 1)
            {
                PointCloud pc1 = pointCloudGenerator.pointClouds[0];
                PointCloud pc2 = pointCloudGenerator.pointClouds[1];
                pc1.morphTexture = pc2.positionTextureMap;
                pc2.morphTexture = pc1.positionTextureMap;
                pc1.GetComponent<VisualEffect>().SetTexture("TargetPosMapTex", pc2.positionTextureMap);
                pc2.GetComponent<VisualEffect>().SetTexture("TargetPosMapTex", pc1.positionTextureMap);
                pc1.otherName = pc2.GraphName;
                pc2.otherName = pc1.GraphName;
                pc1.originalName = pc1.GraphName;
                pc2.otherName = pc1.GraphName;
                pc2.originalName = pc2.GraphName;
            }

            attributeReader = gameObject.AddComponent<AttributeReader>();
            attributeReader.referenceManager = referenceManager;
            StartCoroutine(attributeReader.ReadAttributeFilesCoroutine(fullPath));
            while (!attributeFileRead)
                yield return null;
            referenceManager.attributeSubMenu.SwitchButtonStates(true);
            ReadFacsFiles(path);
            ReadFilterFiles(CellexalUser.UserSpecificFolder);
            StartCoroutine(pointCloudGenerator.ReadMetaData(fullPath));
            while (pointCloudGenerator.readingFile)
                yield return null;
            StartCoroutine(pointCloudGenerator.CreateColorTextureMap());

            while (pointCloudGenerator.creatingGraph)
                yield return null;

            PointCloud parentPC = pointCloudGenerator.pointClouds[0];

            files = Directory.GetFiles(fullPath, "*images.csv");
            string[] imageFiles = Directory.GetFiles(fullPath, "*.png");
            for (int i = 0; i < files.Length; i++)
            {
                int lineCount = 0;
                string imageFile = imageFiles[i];
                byte[] imageData = File.ReadAllBytes(imageFile);
                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(imageData);
                string[] names = files[i].Split(Path.DirectorySeparatorChar);
                string n = names[names.Length - 1].Split('.')[0];
                HistoImage hi = pointCloudGenerator.CreateNewHistoImage();
                hi.sliceNr = int.Parse(Regex.Match(n, @"\d+").Value);
                hi.gameObject.name = n;
                hi.texture = texture;
                hi.transform.position = new Vector3(0f, 1f, (float)i / 5f);
                using (StreamReader sr = new StreamReader(files[i]))
                {
                    string header = sr.ReadLine();
                    while (!sr.EndOfStream)
                    {
                        string line = sr.ReadLine();
                        string[] words = line.Split(',');
                        string cellName = words[0];
                        float.TryParse(words[1], out float x);
                        float.TryParse(words[2], out float y);
                        int xCoord = (int)x;
                        int yCoord = (int)y;
                        PointCloudGenerator.instance.AddGraphPoint(cellName, x, y);
                        if (lineCount % 100 == 0) yield return null;
                        int textureX = lineCount % PointCloudGenerator.textureWidth;
                        int textureY = (lineCount / PointCloudGenerator.textureWidth);
                        hi.textureCoords[cellName] = new Vector2Int(textureX, textureY);
                        //TextureHandler.instance.textureCoordDict[cellName] = new Vector2Int(textureX, textureY);
                        lineCount++;
                    }
                }
                //hi.Initialize();
                MeshRenderer mr = hi.image.GetComponent<MeshRenderer>();
                mr.material.mainTexture = texture;
                mr.material.SetTexture("_EmissionMap", texture);
                PointCloudGenerator.instance.SpawnPoints(hi, parentPC);
                HistoImageHandler.instance.images.Add(hi);
                string c = hi.sliceNr.ToString();
                var carr = c.ToCharArray();
                string id = n.Split(carr)[0];
                if (!HistoImageHandler.instance.imageDict.ContainsKey(id))
                {
                    HistoImageHandler.instance.imageDict.Add(id, new List<HistoImage>());
                }
                print($"{id}, {hi.gameObject.name}");
                HistoImageHandler.instance.imageDict[id].Add(hi);

            }

            CellexalEvents.GraphsLoaded.Invoke();
        }

        /// <summary>
        /// Reads one folder of data and creates the graphs described by the data.
        /// </summary>
        /// <param name="path"> The path to the folder. </param>
        //[ConsoleCommand("inputReader", folder: "Data", aliases: new string[] { "readfolder", "rf" })]
        public void ReadFolder(string path, Dictionary<string, string> h5config = null, string fromPreviousSession = "")
        {
            currentPath = path;
            string workingDirectory = Directory.GetCurrentDirectory();
            string fullPath = workingDirectory + "\\Data\\" + path;
            if (Directory.Exists(workingDirectory + "\\Output"))
            {
                if (File.Exists(workingDirectory + "\\Output\\r_log.txt"))
                {
                    CellexalLog.Log("Deleting old r log file");
                    File.Delete(workingDirectory + "\\Output\\r_log.txt");
                }
            }

            UpdateSelectionToolHandler();
            attributeFileRead = false;
            CellexalLog.Log("Started reading the data folder at " + CellexalLog.FixFilePath(fullPath));
            CellexalUser.DataSourceFolder = currentPath;
            selectionManager.DataDir = fullPath;
            if (!debug)
            {
                // clear the network folder
                string networkDirectory = (CellexalUser.UserSpecificFolder + "\\Resources\\Networks").FixFilePath();
                if (!Directory.Exists(networkDirectory))
                {
                    CellexalLog.Log("Creating directory " + CellexalLog.FixFilePath(networkDirectory));
                    Directory.CreateDirectory(networkDirectory);
                }

                string[] networkFilesList = Directory.GetFiles(networkDirectory, "*");
                CellexalLog.Log("Deleting " + networkFilesList.Length + " files in " +
                                CellexalLog.FixFilePath(networkDirectory));
                foreach (string f in networkFilesList)
                {
                    File.Delete(f);
                }
            }

            //summertwerk
            bool h5 = Directory.EnumerateFiles("Data\\" + path, "*.h5ad").Any();
            bool loom = Directory.EnumerateFiles("Data\\" + path, "*.loom").Any();
            if (h5 || loom)
            {
                ReadFileH5(path, h5config);
                return;
            }

            database.InitDatabase(fullPath + "\\database.sqlite");
            string[] mdsFiles = Directory.GetFiles(fullPath,
                CrossSceneInformation.Tutorial ? "DDRTree.mds" : "*.mds");

            if (mdsFiles.Length == 0)
            {
                CellexalError.SpawnError("Empty dataset",
                    "The loaded dataset did not contain any .mds files. Make sure you have placed the dataset files in the correct folder.");
                throw new System.InvalidOperationException("Empty dataset");
            }
            // PDF reading (conversion to images) causing program to crash. Unknown problem... 
            //pdfMesh.ReadPDF(fullPath);

            CellexalLog.Log("Reading " + mdsFiles.Length + " .mds files");
            mdsReader.referenceManager = referenceManager;
            StartCoroutine(mdsReader.ReadMDSFiles(fullPath, mdsFiles));
            StartCoroutine(referenceManager.inputReader.StartServer("main", fromPreviousSession));

            graphGenerator.isCreating = true;
            referenceManager.configManager.ReadConfigFiles(fullPath);
            // multiple_exp if (currentPath.Length > 0)
            // multiple_exp {
            // multiple_exp     currentPath += "+" + path;
            // multiple_exp }
            //LoadPreviousGroupings();
            //string[] spatialMds = Directory.GetFiles(fullPath, "*.spatialmds");
            //StartCoroutine(ReadSpatialMDSFiles(fullPath, spatialMds));
            if (PhotonNetwork.isMasterClient)
            {
                referenceManager.configManager.MultiUserSynchronise();
            }
        }

        private void UpdateSelectionToolHandler()
        {
            referenceManager.heatmapGenerator.selectionManager = referenceManager.selectionManager;
            referenceManager.networkGenerator.selectionManager = referenceManager.selectionManager;
            referenceManager.graphManager.selectionManager = referenceManager.selectionManager;
        }

        /// <summary>
        /// Reads a facs marker file.
        /// </summary>
        /// <param name="path">The path to the directory</param>
        /// <param name="file">The path to the file.</param>
        public void ReadGraphFromMarkerFile(string path, string file)
        {
            facsGraphCounter++;
            StartCoroutine(mdsReader.ReadMDSFiles(path, new string[] { file }, GraphGenerator.GraphType.FACS, false));
        }

        public void ReadFilterFiles(string path)
        {
            string[] files = Directory.GetFiles(path, "*.fil");
            referenceManager.filterMenu.CreateButtons(files);
        }

        /// <summary>
        /// Start the R session that will run in the background. 
        /// </summary>
        /// <param name="serverType">If you are running several sessions give a serverType name that works as a prefix so the 
        /// R session knows which file to look for.</param>
        /// <param name="fromPreviousSession">If you are reloading a previous session then the r object to load is found in that folder instead.</param>
        /// <returns></returns>
        public IEnumerator StartServer(string serverType, string fromPreviousSession = "")
        {
            Process currentProcess = Process.GetCurrentProcess();
            int pid = currentProcess.Id;
            string rScriptFilePath = Application.streamingAssetsPath + @"\R\start_server.R";
            string serverName = CellexalUser.UserSpecificFolder + "\\" + serverType + "Server";
            string dataSourceFolder;
            if (!fromPreviousSession.Equals(string.Empty))
            {
                dataSourceFolder = fromPreviousSession;
            }
            else
            {
                dataSourceFolder = Directory.GetCurrentDirectory() + @"\Data\" + CellexalUser.DataSourceFolder;
            }

            string args = serverName + " " + dataSourceFolder + " " +
                          CellexalUser.UserSpecificFolder + " " + pid;
            CellexalLog.Log("Running start server script at " + rScriptFilePath + " with the arguments " + args);
            string value = null;
            Thread t = new Thread(
                () => { value = RScriptRunner.RunFromCmd(rScriptFilePath, args, true); });
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            t.Start();
            while (!File.Exists(serverName + ".pid"))
            {
                if (value != null && !value.Equals(string.Empty))
                {
                    CellexalError.SpawnError("Failed to start R Server",
                        "Make sure you have set the correct R path in the launcher menu");
                    yield break;
                }

                yield return null;
            }

            stopwatch.Stop();
            CellexalLog.Log("Start Server finished in " + stopwatch.Elapsed.ToString());
            referenceManager.notificationManager.SpawnNotification(serverType + " R Server Session Initiated.");
            StartCoroutine(referenceManager.reportManager.LogStart());
        }

        /// <summary>
        /// To clean up server files after termination.
        /// Can be called if the user wants to start a new session (e.g. when loading a new dataset) or when exiting the program. 
        /// </summary>
        public void QuitServer()
        {
            File.Delete(CellexalUser.UserSpecificFolder + "\\mainServer.pid");
            File.Delete(CellexalUser.UserSpecificFolder + "\\mainServer.input.lock");
            File.Delete(CellexalUser.UserSpecificFolder + "\\mainServer.input.R");

            if (h5readers != null)
            {
                List<string> h5ReadersToRemove = new List<string>();
                foreach (KeyValuePair<string, H5Reader> kvp in h5readers)
                {
                    kvp.Value.CloseConnection();
                    Destroy(kvp.Value.gameObject);
                    h5ReadersToRemove.Add(kvp.Key);
                }

                foreach (string reader in h5ReadersToRemove)
                    h5readers.Remove(reader);
            }
            //File.Delete(CellexalUser.UserSpecificFolder + "\\geneServer.pid");
            CellexalLog.Log("Stopped Server");
        }

        /// <summary>
        /// Reads the index.facs file.
        /// </summary>
        public void ReadFacsFiles(string path)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            string fullPath = path + "\\index.facs";

            if (!File.Exists(fullPath))
            {
                print("File " + fullPath + " not found");
                CellexalLog.Log(".facs file not found");
                return;
            }

            FileStream fileStream = new FileStream(fullPath, FileMode.Open);
            StreamReader streamReader = new StreamReader(fileStream);

            // The file format should be:
            //             TYPE_1  TYPE_2 ...
            // CELLNAME_1  VALUE   VALUE  
            // CELLNAME_2  VALUE   VALUE
            // ...

            string headerLine = streamReader.ReadLine();
            if (headerLine == null)
            {
                // empty file
                CellexalLog.Log("Empty index.facs file");
                return;
            }

            string[] header = headerLine.Split(new string[] { "\t", " " }, StringSplitOptions.RemoveEmptyEntries);
            float[] min = new float[header.Length];
            float[] max = new float[header.Length];
            string[] values = new string[header.Length + 1];
            int i = 0;
            for (; i < min.Length; ++i)
            {
                min[i] = float.MaxValue;
                max[i] = float.MinValue;
            }

            for (i = 0; !streamReader.EndOfStream; ++i)
            {
                string line = streamReader.ReadLine();
                SplitValues(line, ref values, separators);
                string cellName = values[0];
                for (int j = 0; j < values.Length - 1; ++j)
                {
                    //float value;
                    //try
                    //{
                    //    value = float.Parse(values[j + 1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                    //}
                    //catch (Exception e)
                    //{
                    //    print($"could not parse {cellName}, {values[j+1]}, {values[j]}, {values[j + 2]}");
                    //    value = 0f;
                    //}
                    float.TryParse(values[j + 1],
                        System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture.NumberFormat,
                        out float value);
                    if (value < min[j])
                        min[j] = value;
                    if (value > max[j])
                        max[j] = value;
                }

                for (int j = 0; j < values.Length - 1; ++j)
                {
                    // normalize to the range [0, 29]
                    cellManager.AddFacs(cellName, header[j],
                        float.Parse(values[j + 1], System.Globalization.CultureInfo.InvariantCulture.NumberFormat));
                    cellManager.AddFacsValue(cellName, header[j], values[j + 1]);
                }
            }

            // set the min and max values
            for (i = 0; i < header.Length; ++i)
            {
                cellManager.FacsRanges[header[i].ToLower()] = new Tuple<float, float>(min[i], max[i]);
            }

            streamReader.Close();
            fileStream.Close();
            indexMenu.CreateButtons(header);
            createFromMarkerMenu.CreateButtons(header);
            cellManager.Facs = header;
            CellexalLog.Log("Successfully read " + CellexalLog.FixFilePath(fullPath));
            stopwatch.Stop();
        }

        /// <summary>
        /// Read numerical attribute data e.g. age in geomx data (not facs or gene expression those are read seperately)
        /// </summary>
        /// <param name="path"></param>
        public void ReadNumericalData(string path)
        {
            string fullPath = path + "\\numerical.csv";
            if (!File.Exists(fullPath))
                return;
            using (StreamReader sr = new StreamReader(fullPath))
            {
                string headerLine = sr.ReadLine();
                if (headerLine == null)
                {
                    // empty file
                    CellexalLog.Log("Empty index.facs file");
                    return;
                }

                string[] header = headerLine.Split(new string[] { "\t", " " }, StringSplitOptions.RemoveEmptyEntries);
                float[] min = new float[header.Length];
                float[] max = new float[header.Length];
                string[] values = new string[header.Length + 1];
                int i = 0;
                for (; i < min.Length; ++i)
                {
                    min[i] = float.MaxValue;
                    max[i] = float.MinValue;
                }

                for (i = 0; !sr.EndOfStream; ++i)
                {
                    string line = sr.ReadLine();
                    SplitValues(line, ref values, separators);
                    string cellName = values[0];
                    for (int j = 0; j < values.Length - 1; ++j)
                    {
                        float value = float.Parse(values[j + 1],
                            System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
                        if (value < min[j])
                            min[j] = value;
                        if (value > max[j])
                            max[j] = value;
                    }
                    for (int j = 0; j < values.Length - 1; ++j)
                    {
                        cellManager.AddNumericalAttribute(cellName, header[j], float.Parse(values[j + 1]));
                    }
                }

                for (i = 0; i < header.Length; ++i)
                {
                    cellManager.NumericalAttributeRanges[header[i].ToLower()] = new Tuple<float, float>(min[i], max[i]);
                }
                cellManager.NumericalAttributes = header;

            }
        }

        private void SplitValues(string line, ref string[] values, char[] separators)
        {
            int charIndex = 0;
            for (int i = 0; i < values.Length; ++i)
            {
                int nextSeparator = line.IndexOfAny(separators, charIndex);
                if (nextSeparator >= 0)
                {
                    values[i] = line.Substring(charIndex, nextSeparator - charIndex);
                }
                else
                {
                    values[i] = line.Substring(charIndex);
                }

                charIndex = nextSeparator + 1;
            }
        }

        /// <summary>
        /// Reads the files containg networks.
        /// </summary>
        public void ReadNetworkFiles(int layoutSeed, string path, string selectionFile)
        {
            if (!networkReader)
            {
                networkReader = gameObject.AddComponent<NetworkReader>();
                networkReader.referenceManager = referenceManager;
            }

            StartCoroutine(networkReader.ReadNetworkFilesCoroutine(layoutSeed, path, selectionFile));
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
                CellexalLog.Log(
                    "WARNING: No groupings info file found at " + CellexalLog.FixFilePath(groupingsInfoFile));
                return;
            }

            FileStream fileStream = new FileStream(groupingsInfoFile, FileMode.Open);
            StreamReader streamReader = new StreamReader(fileStream);
            // skip the header
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
            Dictionary<int, UnityEngine.Color>[] groupingColors =
                new Dictionary<int, UnityEngine.Color>[groupingNames.Count];
            for (int i = 0; i < cellNames.Length; ++i)
            {
                cellNames[i] = new string[fileLengths[i]];
                groups[i] = new int[fileLengths[i]];
            }

            for (int i = 0; i < groupingNames.Count; ++i)
            {
                groupingColors[i] = new Dictionary<int, UnityEngine.Color>();
            }

            words = null;
            string[] files = Directory.GetFiles(dataFolder, "selection*.txt");
            for (int i = 0; i < fileLengths.Count; ++i)
            {
                string file = files[i];
                fileStream = new FileStream(file, FileMode.Open);
                streamReader = new StreamReader(fileStream);

                for (int j = 0; j < fileLengths[i]; ++j)
                {
                    line = streamReader.ReadLine();
                    print(line);
                    words = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    cellNames[i][j] = words[0];

                    try
                    {
                        int group = int.Parse(words[3]);
                        groups[i][j] = group;
                        UnityEngine.Color groupColor;
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

            // referenceManager.selectionFromPreviousMenu.SelectionFromPreviousButton(graphNames, groupingNames.ToArray(),
            // cellNames, groups, groupingColors);
            CellexalLog.Log("Successfully read " + groupingNames.Count + " files");
        }

        /// <summary>
        /// Read input from annotation file. File should contain cell ids in one column and annotation in one.
        /// </summary>
        /// <param name="path">Path to annotation file.</param>
        public void ReadAnnotationFile(string path)
        {
            //string dataFolder = CellexalUser.UserSpecificFolder;
            if (!File.Exists(path))
            {
                CellexalLog.Log("Could not find file:" + path);
                return;
            }

            FileStream fileStream = new FileStream(path, FileMode.Open);
            StreamReader streamReader = new StreamReader(fileStream);
            List<Cell> cellsToAnnotate = new List<Cell>();
            Graph graph = referenceManager.graphManager.Graphs[0];
            int numPointsAdded = 0;
            string line = streamReader.ReadLine();
            string[] words = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            string firstCellName = words[0];
            cellsToAnnotate.Add(referenceManager.cellManager.GetCell(firstCellName));
            string annotation = words[1];
            while (!streamReader.EndOfStream)
            {
                line = streamReader.ReadLine();
                words = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (words[1] != annotation)
                {
                    referenceManager.annotationManager.AddAnnotation(annotation, cellsToAnnotate, graph.FindGraphPoint(cellsToAnnotate[0].Label).Position, path);
                    cellsToAnnotate.Clear();
                    annotation = words[1];
                }

                cellsToAnnotate.Add(referenceManager.cellManager.GetCell(words[0]));
                numPointsAdded++;
            }

            referenceManager.annotationManager.AddAnnotation(annotation, cellsToAnnotate, graph.FindGraphPoint(cellsToAnnotate[0].Label).Position, path);
            cellsToAnnotate.Clear();
            CellexalLog.Log($"Added {numPointsAdded} points to annotation");
            CellexalEvents.CommandFinished.Invoke(true);
            streamReader.Close();
            fileStream.Close();
        }

        /// <summary>
        /// Make selection from reading either previously made selection in session or an externally made selection.
        /// File contains cell ids and group colour and name of graph it was selected from.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="select"></param>
        /// <returns></returns>
        [ConsoleCommand("inputReader", aliases: new string[] { "readselectionfile", "rsf" })]
        public List<Graph.GraphPoint> ReadSelectionFile(string path, bool select = true)
        {
            //string dataFolder = CellexalUser.UserSpecificFolder;
            List<Graph.GraphPoint> selection = new List<Graph.GraphPoint>();
            if (!File.Exists(path))
            {
                CellexalLog.Log("Could not find file:" + path);
                return new List<Graph.GraphPoint>();
            }

            FileStream fileStream = new FileStream(path, FileMode.Open);
            StreamReader streamReader = new StreamReader(fileStream);
            SelectionManager selectionManager = referenceManager.selectionManager;
            selectionManager.CancelSelection();
            GraphManager graphManager = referenceManager.graphManager;
            int numPointsAdded = 0;
            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();
                string[] words = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                int group;
                UnityEngine.Color groupColor;

                try
                {
                    // group = int.Parse(words[3]);
                    string colorString = words[1];
                    ColorUtility.TryParseHtmlString(colorString, out groupColor);
                    if (!CellexalConfig.Config.SelectionToolColors.Any(x => CompareColor(x, groupColor)))
                    {
                        referenceManager.settingsMenu.AddSelectionColor(groupColor);
                        referenceManager.settingsMenu.unsavedChanges = false;
                    }
                    group = referenceManager.selectionToolCollider.GetColorIndex(groupColor);
                }
                catch (FormatException)
                {
                    CellexalLog.Log(string.Format("Bad color on line {0} in file {1}.", numPointsAdded + 1,
                        path));
                    streamReader.Close();
                    fileStream.Close();
                    CellexalEvents.CommandFinished.Invoke(false);
                    return new List<Graph.GraphPoint>();
                }

                Graph.GraphPoint graphPoint = graphManager.FindGraphPoint(words[2], words[0]);
                selection.Add(graphPoint);
                if (select)
                {
                    selectionManager.AddGraphpointToSelection(graphManager.FindGraphPoint(words[2], words[0]),
                        group, false, groupColor);
                    numPointsAdded++;
                }
            }

            CellexalLog.Log(string.Format("Added {0} points to selection", numPointsAdded));
            CellexalEvents.CommandFinished.Invoke(true);
            CellexalEvents.SelectedFromFile.Invoke();
            streamReader.Close();
            fileStream.Close();
            return selection;
        }


        [ConsoleCommand("inputReader", aliases: new string[] { "selectfromprevious", "sfp" })]
        public void ReadAndSelectPreviousSelection(int index)
        {
            string dataFolder = CellexalUser.UserSpecificFolder;
            string[] files = Directory.GetFiles(dataFolder, "selection*.txt");
            if (files.Length == 0)
            {
                CellexalLog.Log("No previous selections found.");
                CellexalEvents.CommandFinished.Invoke(false);
                return;
            }
            else if (index < 0 || index >= files.Length)
            {
                CellexalLog.Log(string.Format(
                    "Index \'{0}\' is not within the range [0, {1}] when reading previous selection files.", index,
                    files.Length - 1));
                CellexalEvents.CommandFinished.Invoke(false);
                return;
            }

            string path = files[index];
            ReadSelectionFile(path);
        }

        /// <summary>
        /// Helper function used to decide if two colors are (by some margin) equal.
        /// E.g. Used when reading in selections and deciding if a new color needs to be added or if it already exists in the config.
        /// Alpha channel is ignored.
        /// </summary>
        /// <param name="a">First color.</param>
        /// <param name="b">Second color.</param>
        /// <param name="tolerance">Function returns true if the distance between them are equal or lower than this value.</param>
        public static bool CompareColor(UnityEngine.Color a, UnityEngine.Color b, float tolerance = 0.1f)
        {
            float diff = Vector3.Distance(new Vector3(a.r, a.g, a.b),
                new Vector3(b.r, b.g, b.b));
            return diff <= tolerance;
        }
    }
}