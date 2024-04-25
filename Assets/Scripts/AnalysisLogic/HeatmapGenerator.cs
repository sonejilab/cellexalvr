using CellexalVR.AnalysisObjects;
using CellexalVR.DesktopUI;
using CellexalVR.Extensions;
using CellexalVR.General;
using CellexalVR.Menu.Buttons;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace CellexalVR.AnalysisLogic
{
    /// <summary>
    /// A generator for heatmaps.
    /// </summary>
    public class HeatmapGenerator : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject heatmapPrefab;
        public GameObject heatmapTexturePrefab;
        public Texture2D heatmapTexture;
        public int selectionNr;
        public int heatmapsCreated = 0;
        public bool GeneratingHeatmaps { get; private set; }
        public SolidBrush[] expressionColors;
        public SelectionManager selectionManager;
        public Vector3[] startPositions;

        private Thread t;
        private Vector3 heatmapPosition;
        private List<Heatmap> heatmapList = new List<Heatmap>();
        private string statsMethod;
        private System.Drawing.Font geneFont;
        private int nrOfGenes = 65;
        private int numHeatmapTextures;
        private int textureWidth;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        [ConsoleCommand("heatmapGenerator", folder: "Output", aliases: new string[] { "loadheatmapfile", "lhf" })]
        public void LoadHeatmap(string heatmapName, string fromSelectionFile = "")
        {
            string[] words = heatmapName.Split(Path.DirectorySeparatorChar);
            string heatmapNameLastPart = words[words.Length - 1];
            string directory = Path.Combine(CellexalUser.UserSpecificFolder, "Heatmap", heatmapNameLastPart);
            CellexalLog.Log("Loading old heatmap from file " + directory);
            Heatmap heatmap = Instantiate(heatmapPrefab).GetComponent<Heatmap>();
            heatmap.selection = referenceManager.inputReader.ReadSelectionFile(fromSelectionFile, false);
            heatmap.Init();
            heatmap.transform.parent = transform;
            heatmap.transform.localPosition = heatmapPosition;
            heatmapList.Add(heatmap);
            heatmap.directory = directory;
            BuildTexture(heatmap.selection, heatmapName, heatmap);
            heatmapNameLastPart = heatmapNameLastPart.Replace(".txt", "");
            GameObject existingHeatmap = GameObject.Find(heatmapNameLastPart);
            while (existingHeatmap != null)
            {
                heatmapNameLastPart += "_Copy";
                existingHeatmap = GameObject.Find(heatmapNameLastPart);
            }

            heatmap.gameObject.name = heatmapNameLastPart;
        }

        void Awake()
        {
            int posCount = 6;
            startPositions = new Vector3[posCount];
            double angleStep = (-2.2f * Mathf.PI) / (float)(posCount + 1);
            double angle;
            for (int i = 0; i < posCount; i++)
            {
                angle = -1.7f * Mathf.PI + angleStep + (float)i * angleStep;
                Vector3 pos = new Vector3(Mathf.Cos((float)angle) * 2f, 1.5f, Mathf.Sin((float)angle) * 2f);
                startPositions[i] = pos;
            }

            t = null;
            heatmapPosition = heatmapPrefab.transform.position;
            selectionManager = referenceManager.selectionManager;
            GeneratingHeatmaps = false;
            CellexalEvents.ConfigLoaded.AddListener(InitColors);
        }

        /// <summary>
        /// Initializes <see cref="expressionColors"/> with the colors in the config file.
        /// </summary>
        public void InitColors()
        {
            int numberOfExpressionColors = CellexalConfig.Config.NumberOfHeatmapColors;
            expressionColors = new SolidBrush[numberOfExpressionColors];
            UnityEngine.Color low = CellexalConfig.Config.HeatmapLowExpressionColor;
            UnityEngine.Color mid = CellexalConfig.Config.HeatmapMidExpressionColor;
            UnityEngine.Color high = CellexalConfig.Config.HeatmapHighExpressionColor;

            int dividerLowMid = numberOfExpressionColors / 2;
            if (dividerLowMid == 0)
                dividerLowMid = 1;
            float lowMidDeltaR = (mid.r * mid.r - low.r * low.r) / dividerLowMid;
            float lowMidDeltaG = (mid.g * mid.g - low.g * low.g) / dividerLowMid;
            float lowMidDeltaB = (mid.b * mid.b - low.b * low.b) / dividerLowMid;

            int dividerMidHigh = numberOfExpressionColors - dividerLowMid - 1;
            if (dividerMidHigh == 0)
                dividerMidHigh = 1;
            float midHighDeltaR = (high.r * high.r - mid.r * mid.r) / dividerMidHigh;
            float midHighDeltaG = (high.g * high.g - mid.g * mid.g) / dividerMidHigh;
            float midHighDeltaB = (high.b * high.b - mid.b * mid.b) / dividerMidHigh;

            for (int i = 0; i < numberOfExpressionColors / 2 + 1; ++i)
            {
                float r = low.r * low.r + lowMidDeltaR * i;
                float g = low.g * low.g + lowMidDeltaG * i;
                float b = low.b * low.b + lowMidDeltaB * i;
                if (r < 0) r = 0;
                if (g < 0) g = 0;
                if (b < 0) b = 0;
                expressionColors[i] = new SolidBrush(System.Drawing.Color.FromArgb((int)(Mathf.Sqrt(r) * 255),
                    (int)(Mathf.Sqrt(g) * 255), (int)(Mathf.Sqrt(b) * 255)));
            }

            for (int i = numberOfExpressionColors / 2 + 1, j = 1; i < numberOfExpressionColors; ++i, ++j)
            {
                float r = mid.r * mid.r + midHighDeltaR * j;
                float g = mid.g * mid.g + midHighDeltaG * j;
                float b = mid.b * mid.b + midHighDeltaB * j;
                if (r < 0) r = 0;
                if (g < 0) g = 0;
                if (b < 0) b = 0;
                expressionColors[i] = new SolidBrush(System.Drawing.Color.FromArgb((int)(Mathf.Sqrt(r) * 255),
                    (int)(Mathf.Sqrt(g) * 255), (int)(Mathf.Sqrt(b) * 255)));
            }

        }

        /// <summary>
        /// Removes all heatmaps.
        /// </summary>
        internal void DeleteHeatmaps()
        {
            foreach (Heatmap h in heatmapList)
            {
                if (h != null)
                {
                    Destroy(h.gameObject);
                }
            }

            heatmapList.Clear();
        }

        [ConsoleCommand("heatmapGenerator", aliases: new string[] { "generateheatmap", "gh" })]
        public void CreateHeatmap()
        {
            statsMethod = CellexalConfig.Config.HeatmapAlgorithm;
            CellexalLog.Log("Creating heatmap");
            CellexalEvents.CreatingHeatmap.Invoke();
            string heatmapName = "heatmap_" + System.DateTime.Now.ToString("HH-mm-ss");
            Selection selection = ReferenceManager.instance.selectionManager.GetLastSelection();
            StartCoroutine(GenerateHeatmapRoutine(selection, heatmapName));
        }

        /// <summary>
        /// Creates a new heatmap using the last confirmed selection.
        /// </summary>
        /// <param name="heatmapName">If created via multiplayer. Name it the same as on other client.</param>
        public void CreateHeatmap(string heatmapName = "")
        {
            if (heatmapName.Equals(string.Empty))
            {
                heatmapName = "heatmap_" + System.DateTime.Now.ToString("HH-mm-ss");
                referenceManager.multiuserMessageSender.SendMessageCreateHeatmap(heatmapName);
            }

            statsMethod = CellexalConfig.Config.HeatmapAlgorithm;
            CellexalLog.Log("Creating heatmap");
            CellexalEvents.CreatingHeatmap.Invoke();
            Selection selection = ReferenceManager.instance.selectionManager.GetLastSelection();
            StartCoroutine(GenerateHeatmapRoutine(selection, heatmapName));
        }

        /// <summary>
        /// Removes one heatmap.
        /// </summary>
        /// <param name="heatmapName">The heatmap's name.</param>
        public void DeleteHeatmap(string heatmapName)
        {
            Heatmap heatmap = FindHeatmap(heatmapName);
            heatmapList.Remove(heatmap);
            heatmap.DeleteHeatmap();
        }

        /// <summary>
        /// Finds a heatmap.
        /// </summary>
        /// <param name="heatmapName">The heatmap's name.</param>
        /// <returns>The heatmap, or null if no heatmap by that name exists.</returns>
        public Heatmap FindHeatmap(string heatmapName)
        {
            return heatmapList.Find((Heatmap h) => h != null ? heatmapName == h.name : false);
        }

        /// <summary>
        /// Highlights gene in genelist in all the heatmaps if it is there.
        /// </summary>
        /// <param name="geneName">Name of the gene to be highlighted.</param>
        public void HighLightGene(string geneName)
        {
            foreach (Heatmap hm in heatmapList)
            {
                hm.HighLightGene(geneName);
            }
        }


        /// <summary>
        /// Coroutine for creating a heatmap.
        /// </summary>
        private IEnumerator GenerateHeatmapRoutine(Selection selection, string heatmapName = "")
        {
            GeneratingHeatmaps = true;
            referenceManager.floor.StartPulse();
            if (!ScarfManager.instance.scarfActive)
            {
                while (selectionManager.RObjectUpdating)
                {
                    yield return null;
                }
            }

            if (selection.size < 1)
            {
                CellexalLog.Log("can not create heatmap with less than 1 graphpoints, aborting");
                if (!(referenceManager.networkGenerator.GeneratingNetworks &&
                      File.Exists(Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.R"))))
                {
                    referenceManager.floor.StopPulse();
                }

                referenceManager.notificationManager.SpawnNotification("Heatmap generation failed.");
                yield break;
            }

            string heatmapDirectory = Path.Combine(CellexalUser.UserSpecificFolder, "Heatmap");
            string outputFilePath = Path.Combine(heatmapDirectory, heatmapName + ".txt");

            if (!ScarfManager.instance.scarfActive)
            {
                string objectPath = Path.Combine(CellexalUser.UserSpecificFolder, "cellexalObj.RData");

                string topGenesNr = nrOfGenes.ToString();
                string statsMethod = CellexalConfig.Config.HeatmapAlgorithm;
                // the check for the txt.time file needs to wait for the R to create it.
                string groupingFilePath = selection.savedSelectionFilePath;
                string args = CellexalUser.UserSpecificFolder.MakeDoubleBackslash() + " " +
                    groupingFilePath.MakeDoubleBackslash() + " " +
                    topGenesNr + " " +
                    outputFilePath.MakeDoubleBackslash() + " " +
                    statsMethod;

                string rScriptFilePath = Path.Combine(Application.streamingAssetsPath, "R", "make_heatmap.R");

                if (!Directory.Exists(heatmapDirectory))
                {
                    CellexalLog.Log("Creating directory " + heatmapDirectory);
                    Directory.CreateDirectory(heatmapDirectory);
                }
                string mainserverPidPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.pid");
                string mainserverInputPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.R");
                string mainserverInputLockPath = Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.lock");

                bool rServerReady = File.Exists(mainserverPidPath) &&
                                    !File.Exists(mainserverInputPath) &&
                                    !File.Exists(mainserverInputLockPath);
                while (!rServerReady || !RScriptRunner.serverIdle)
                {
                    rServerReady = File.Exists(mainserverPidPath) &&
                                    !File.Exists(mainserverInputPath) &&
                                    !File.Exists(mainserverInputLockPath);
                    yield return null;
                }

                //t = new Thread(() => RScriptRunner.RunScript(script));
                t = new Thread(() => RScriptRunner.RunRScript(rScriptFilePath, args));
                t.Start();
                CellexalLog.Log("Running R function " + rScriptFilePath + " with the arguments: " + args);
                var stopwatch = new System.Diagnostics.Stopwatch();
                stopwatch.Start();

                while (t.IsAlive || File.Exists(mainserverInputPath))
                {
                    yield return null;
                }

                while (!File.Exists(outputFilePath))
                {
                    yield return null;
                }
            }
            else
            {
                //yield return ScarfManager.instance.RunMarkerSearch("RNA_leiden_cluster", 0.25f);
                yield return ScarfManager.instance.GetMarkers("RNA_leiden_cluster");
            }
            // stopwatch.Stop();
            // CellexalLog.Log("Heatmap R script finished in " + stopwatch.Elapsed.ToString());

            GeneratingHeatmaps = false;

            var heatmap = Instantiate(heatmapPrefab).GetComponent<Heatmap>();

            heatmap.Init();
            heatmap.transform.parent = transform;
            heatmap.transform.localPosition = heatmapPosition;
            // R might have created a txt.time selection file
            string timelineFilePath = selection.savedSelectionFilePath + ".time";
            if (selection.groups.Count == 1)
            {
                // expect R to have created a .time file
                if (File.Exists(timelineFilePath))
                {
                    heatmap.selection = referenceManager.inputReader.ReadSelectionFile(timelineFilePath, false);
                }
            }
            //print((selectionNr, heatmap.selectionFile));
            heatmapList.Add(heatmap);
            Dataset.instance.heatmaps.Add(heatmap);
            heatmap.directory = outputFilePath;
            BuildTexture(selection, outputFilePath, heatmap);
            GameObject existingHeatmap = GameObject.Find(heatmapName);
            while (existingHeatmap != null)
            {
                heatmapName += "_Copy";
                existingHeatmap = GameObject.Find(heatmapName);
                yield return null;
            }

            heatmap.gameObject.name = heatmapName; //"heatmap_" + heatmapsCreated;
            CellexalEvents.CommandFinished.Invoke(true);
        }


        #region BuildTexture Methods

        /// <summary>
        /// Builds the heatmap texture.
        /// </summary>
        /// <param name="selection">The selection that the heatmap was created from.</param>
        /// <param name="filepath">A path to the file containing the gene names</param>
        public void BuildTexture(Selection selection, string filepath, Heatmap heatmap, bool newHeatmap = true)
        {
            if (selection == null)
            {
                CellexalLog.Log("WARNING: No selection to build texture from.");
                return;
            }

            heatmap.selection = selection;
            if (heatmap.buildingTexture)
            {
                CellexalLog.Log("WARNING: Not building heatmap texture because it is already building");
                return;
            }

            gameObject.SetActive(true);
            heatmap.GetComponent<Collider>().enabled = false;
            heatmap.cells = new Cell[selection.size];
            heatmap.attributeWidths = new List<Tuple<int, float, int>>();
            heatmap.cellAttributes = new List<Tuple<Cell, int>>();
            heatmap.attributeColors = new Dictionary<int, UnityEngine.Color>();
            heatmap.groupWidths = new List<Tuple<int, float, int>>();
            heatmap.groupingColors = new Dictionary<int, UnityEngine.Color>();
            float cellWidth = (float)heatmap.layout.heatmapWidth / selection.size;
            int lastGroup = -1;
            int groupWidth = 0;
            heatmap.layout.attributeWidth = 0;
            heatmap.layout.lastAttribute = -1;
            // read the cells and their groups
            for (int i = 0; i < selection.size; ++i)
            {
                Graph.GraphPoint graphpoint = selection[i];
                int group = graphpoint.Group;
                var cell = referenceManager.cellManager.GetCell(graphpoint.Label);
                heatmap.cells[i] = cell;
                string firstAttribute = ReferenceManager.instance.cellManager.GetAttributes(cell).First();
                heatmap.AddAttributeWidth(firstAttribute, cellWidth, cell);
                heatmap.groupingColors[group] = graphpoint.GetColor();
                if (lastGroup == -1)
                {
                    lastGroup = group;
                }

                // used for saving the widths of the groups later
                if (group != lastGroup)
                {
                    heatmap.groupWidths.Add(new Tuple<int, float, int>(lastGroup, groupWidth * cellWidth,
                        (int)groupWidth));
                    groupWidth = 0;
                    lastGroup = group;
                }

                groupWidth++;
            }

            // add the last group as well
            heatmap.groupWidths.Add(new Tuple<int, float, int>(lastGroup, groupWidth * cellWidth, groupWidth));
            heatmap.attributeWidths.Add(new Tuple<int, float, int>(heatmap.layout.lastAttribute,
                heatmap.layout.attributeWidth * cellWidth, heatmap.layout.attributeWidth));
            if (heatmap.genes == null || heatmap.genes.Length == 0)
            {
                if (!ScarfManager.instance.scarfActive)
                {
                    if (!File.Exists(filepath))
                    {
                        CellexalLog.Log("Failed to create heatmap. R script did not return gene list file " + filepath);
                        CellexalError.SpawnError("Failed to create heatmap",
                            "R script did not return gene list file " + filepath);
                        if (!(referenceManager.networkGenerator.GeneratingNetworks &&
                              File.Exists(Path.Combine(CellexalUser.UserSpecificFolder, "mainServer.input.R"))))
                        {
                            referenceManager.floor.StopPulse();
                        }

                        Destroy(heatmap.gameObject);

                        return;
                    }

                    StreamReader streamReader = new StreamReader(filepath);
                    int numberOfGenes = int.Parse(streamReader.ReadLine());
                    heatmap.genes = new string[numberOfGenes];
                    int i = 0;
                    while (!streamReader.EndOfStream)
                    {
                        heatmap.genes[i] = streamReader.ReadLine();
                        i++;
                    }

                    streamReader.Close();
                }
                else
                {
                    heatmap.genes = ScarfManager.instance.markers;
                }
            }

            heatmap.orderedByAttribute = false;
            StartCoroutine(BuildTextureCoroutine(heatmap, newHeatmap));
        }

        /// <summary>
        /// Builds a heatmap texture based on already known groupwidths and attribute widths.
        /// </summary>
        /// <param name="heatmap">The heatmap to attach the texture to.</param>
        public void BuildTexture(Heatmap heatmap, bool newHeatmap = true)
        {
            if (heatmap.buildingTexture)
            {
                CellexalLog.Log("WARNING: Not building heatmap texture because it is already building");
                return;
            }

            heatmap.GetComponent<Collider>().enabled = false;
            // merge groups
            List<Tuple<int, float, int>> groupWidths = heatmap.groupWidths;
            for (int i = 0; i < groupWidths.Count - 1; ++i)
            {
                // if two groups with the same color are now beside eachother, merge them
                if (groupWidths[i].Item1 == groupWidths[i + 1].Item1)
                {
                    Tuple<int, float, int> oldTuple1 = groupWidths[i];
                    Tuple<int, float, int> oldTuple2 = groupWidths[i + 1];
                    float newWidthCoords = oldTuple1.Item2 + oldTuple2.Item2;
                    int newWidthCells = oldTuple1.Item3 + oldTuple2.Item3;
                    groupWidths[i] = new Tuple<int, float, int>(oldTuple1.Item1, newWidthCoords, newWidthCells);
                    groupWidths.RemoveAt(i + 1);
                }
            }

            StartCoroutine(BuildTextureCoroutine(heatmap, newHeatmap));
        }

        /// <summary>
        /// Builds this heatmaps texture using the supplied cells and genes.
        /// </summary>
        /// <param name="newCells">The cells that the heatmap should contain.</param>
        /// <param name="newGenes">The genes that the heatmap should contain.</param>
        /// <param name="newGroupWidths">The grouping information.</param>
        public void BuildTexture(Cell[] newCells, string[] newGenes, List<Tuple<int, float, int>> newGroupWidths,
            Heatmap heatmap)
        {
            if (heatmap.buildingTexture)
            {
                CellexalLog.Log("WARNING: Not building heatmap texture because it is already building");
                return;
            }

            heatmap.cells = newCells;
            heatmap.genes = newGenes;
            heatmap.groupWidths = newGroupWidths;
            heatmap.UpdateAttributeWidhts();
            StartCoroutine(BuildTextureCoroutine(heatmap, false));
        }

        /// <summary>
        /// Coroutine that starts a thread and generates the heatmap.
        /// </summary>
        /// <param name="heatmap"></param>
        public IEnumerator BuildTextureCoroutine(Heatmap heatmap, bool newHeatmap = true)
        {
            heatmap.buildingTexture = true;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            CellexalLog.Log("Started building a heatmap texture");
            referenceManager.floor.StartPulse();

            foreach (var button in heatmap.GetComponentsInChildren<CellexalButton>())
            {
                button.SetButtonActivated(false);
            }

            ArrayList res = new ArrayList();
            Dictionary<string, string> geneIds = new Dictionary<string, string>();
            geneIds = new Dictionary<string, string>(res.Count);
            if (!ScarfManager.instance.scarfActive)
            {
                SQLiter.SQLite db = heatmap.gameObject.AddComponent<SQLiter.SQLite>();
                db.referenceManager = referenceManager;
                db.InitDatabase(heatmap.directory + ".sqlite3");

                yield return StartCoroutine(db.ValidateDatabaseCoroutine());
                if (!db._databaseOK)
                {
                    // something went wrong in the r script
                    CellexalError.SpawnError("Could not generate heatmap",
                        "R script did not return a valid gene database file, check R log for more information");
                    CellexalLog.Log("Could not generate heatmap, R script did not return a valid gene database file");
                    Destroy(heatmap.gameObject);
                    referenceManager.floor.StopPulse();
                    yield break;
                }

                while (db.QueryRunning)
                {
                    yield return null;
                }

                db.QueryGenesIds(heatmap.genes);
                while (db.QueryRunning)
                {
                    yield return null;
                }

                res = db._result;
                foreach (Tuple<string, string> t in res)
                {
                    // keys are names, values are ids
                    geneIds[t.Item1] = t.Item2;
                }

                while (db.QueryRunning)
                {
                    yield return null;
                }

                db.QueryGenesInCells(heatmap.genes, heatmap.cells.Select((c) => c.Label).ToArray());
                while (db.QueryRunning)
                {
                    yield return null;
                }

                res = db._result;
            }
            else
            {
                float tolerance = 0.001f;
                int lowestGeneExpressionIndex = 0;
                int highestGeneExpressionIndex = 1;
                for (int i = 0; i < heatmap.genes.Length; i++)
                {
                    string gene = heatmap.genes[i];
                    string geneID = gene; // i.ToString();
                    geneIds[gene] = geneID;
                    yield return ScarfManager.instance.GetCellValues(gene);
                    float[] values = ScarfManager.instance.cellValues;
                    float min = float.PositiveInfinity;
                    float max = 0f;
                    res.Add(new Tuple<string, float>(geneID, min));
                    res.Add(new Tuple<string, float>(geneID, max));
                    lowestGeneExpressionIndex = res.Count - 2;
                    highestGeneExpressionIndex = lowestGeneExpressionIndex + 1;
                    for (int j = 0; j < values.Length; j++)
                    {
                        float v = values[j];
                        if (v - tolerance < 0)
                        {
                            min = 0;
                            continue;
                        }
                        if (v < min)
                        {
                            min = v;
                        }
                        else if (v > max)
                        {
                            max = v;
                        }
                        res.Add(new Tuple<string, float>(j.ToString(), v));
                    }
                    res[lowestGeneExpressionIndex] = new Tuple<string, float>(geneID, min);
                    res[highestGeneExpressionIndex] = new Tuple<string, float>(geneID, max);
                }
            }

            Dictionary<string, int> genePositions = new Dictionary<string, int>(heatmap.genes.Length);
            for (int i = 0; i < heatmap.genes.Length; ++i)
            {
                // gene names are keys, positions are values
                genePositions[heatmap.genes[i]] = i;
            }

            Dictionary<string, int> cellsPosition = new Dictionary<string, int>(heatmap.cells.Length);
            for (int i = 0; i < heatmap.cells.Length; ++i)
            {
                cellsPosition[heatmap.cells[i].Label] = i;
            }

            string heatmapDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Output", "Images");
            if (!Directory.Exists(heatmapDirectory))
            {
                Directory.CreateDirectory(heatmapDirectory);
            }

            string heatmapFilePath = Path.Combine(heatmapDirectory, "heatmap_temp");
            Thread thread;
            //if (ScarfManager.scarfObject != null)
            //{
            //    //Dictionary<string, List<Tuple<string, float>>> result = ScarfManager.GetFeatureValues(heatmap.genes.ToList(), heatmap.cells.Select(c => c.Label).ToList());

            //    // CellexalLog.Log("Reading " + result.Count + " results from database");
            //    thread = new Thread(() => CreateHeatmapPNGScarf(heatmap, result, genePositions, cellsPosition, heatmapFilePath));
            //    thread.Start();
            //}
            thread = new Thread(() => CreateHeatmapPNG(heatmap, res, genePositions, cellsPosition, geneIds, heatmapFilePath));
            thread.Start();
            while (thread.IsAlive)
            {
                yield return null;
            }

            // the thread is now done and the heatmap has been painted
            // copy the bitmap data over to a unity texture
            // using a memorystream here seemed like a better alternative but made the standalone crash
            // these yields makes the loading a little bit smoother, but still cuts a few frames.
            List<GameObject> textureGameObjects = heatmap.layout.textureGameObjects;

            if (textureGameObjects != null)
            {
                foreach (GameObject tex in textureGameObjects)
                {
                    Destroy(tex);
                }

                textureGameObjects.Clear();
            }

            else
            {
                textureGameObjects = new List<GameObject>();
            }

            yield return null;
            Texture2D[] textures = new Texture2D[heatmap.layout.textureGraphics.Count];
            for (int i = 0; i < heatmap.layout.textureGraphics.Count; ++i)
            {
                //Texture2D texture = Instantiate(heatmapTexture) as Texture2D;
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                yield return null;
                texture.LoadImage(File.ReadAllBytes(heatmapFilePath + "_" + i + ".png"));
                texture.filterMode = FilterMode.Point;
                //texture.SetPixels(texture.GetPixels(0, 0, texture.width, texture.height));
                textures[i] = texture;
                yield return null;
                GameObject newTextureGameObject = Instantiate(heatmapTexturePrefab);
                textureGameObjects.Add(newTextureGameObject);
                newTextureGameObject.GetComponent<Renderer>().material.SetTexture("_BaseMap", texture);
                newTextureGameObject.transform.parent = heatmap.transform;
                yield return null;
            }

            if (CurvedHeatmapGenerator.instance != null)
            {
                CurvedHeatmapGenerator.instance.geneListTexture = textures[textures.Length - 1];
                for (int i = 1; i < textures.Length - 1; ++i)
                {
                    CurvedHeatmapGenerator.instance.texture2Ds.Add(textures[i]);
                }
                StartCoroutine(CurvedHeatmapGenerator.instance.GenerateCurvedHeatmap());

            }

            // position left and right side textures
            textureGameObjects[0].transform.localPosition = new Vector3(((float)heatmap.layout.heatmapX / heatmap.layout.bitmapWidth / 2f) - 0.5f, 0f, 0f);
            textureGameObjects[0].transform.localScale = new Vector3(((float)heatmap.layout.heatmapX / heatmap.layout.bitmapWidth), 1f, 1f);
            textureGameObjects[0].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            textureGameObjects[textureGameObjects.Count - 1].transform.localPosition =
                new Vector3((heatmap.layout.geneListX + heatmap.layout.geneListWidth / 2f) / heatmap.layout.bitmapWidth - 0.5f, 0f, 0f);
            textureGameObjects[textureGameObjects.Count - 1].transform.localScale = new Vector3((float)heatmap.layout.geneListWidth / heatmap.layout.bitmapWidth, 1f, 1f);
            textureGameObjects[textureGameObjects.Count - 1].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

            // position middle segment textures
            float textureXStart = (float)heatmap.layout.heatmapX / heatmap.layout.bitmapWidth - 0.5f;
            float textureXInc = (float)(heatmap.layout.bitmapWidth - (heatmap.layout.heatmapX + heatmap.layout.geneListWidth)) / heatmap.layout.bitmapWidth / numHeatmapTextures;
            textureXStart += textureXInc / 2f;
            // since the parent scale is (1, 1, 1) the x-scale (width) of the texture is the same as the distance between two segments
            float textureXScale = textureXInc;
            for (int i = 1; i < textureGameObjects.Count - 1; ++i)
            {
                textureGameObjects[i].transform.localPosition = new Vector3(textureXStart + textureXInc * (i - 1), 0f, 0f);
                textureGameObjects[i].transform.localScale = new Vector3(textureXScale, 1f, 1f);
                textureGameObjects[i].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }

            heatmap.layout.textureGameObjects = textureGameObjects;
            heatmap.GetComponent<Collider>().enabled = true;

            foreach (var button in heatmap.GetComponentsInChildren<CellexalButton>())
            {
                button.SetButtonActivated(true);
            }

            stopwatch.Stop();
            CellexalLog.Log("Finished building a heatmap texture in " + stopwatch.Elapsed.ToString());
            heatmap.buildingTexture = false;

            if (newHeatmap)
            {
                heatmap.startPosition = startPositions[heatmapsCreated % 6];
                heatmapsCreated++;
                heatmap.CreateHeatmapAnimation();
            }


            CellexalEvents.HeatmapCreated.Invoke();
            CellexalEvents.ScriptFinished.Invoke();
            referenceManager.notificationManager.SpawnNotification("Heatmap finished.");
            string sessionEntryName = heatmap.directory + " from " + heatmap.selection.savedSelectionFilePath;
            if (!referenceManager.sessionHistoryList.Contains(sessionEntryName, Definitions.HistoryEvent.HEATMAP))
            {
                referenceManager.sessionHistoryList.AddEntry(heatmap.directory + " from " + heatmap.selection.savedSelectionFilePath,
                    Definitions.HistoryEvent.HEATMAP);
            }
            referenceManager.floor.StopPulse();
        }

        /// <summary>
        /// Helper method that is run as a thread in <see cref="BuildTextureCoroutine(List{Tuple{int, float, int}}, List{Tuple{int, float, int}}, Heatmap)"/>.
        /// </summary>
        public void CreateHeatmapPNG(Heatmap heatmap, ArrayList result, Dictionary<string, int> genePositions,
            Dictionary<string, int> cellsPosition,
            Dictionary<string, string> geneIds, string heatmapFilePath)
        {
            var attributeWidths = heatmap.attributeWidths;
            var groupWidths = heatmap.groupWidths;
            heatmap.layout.textureGraphics = new List<System.Drawing.Graphics>();
            heatmap.layout.textureBitmaps = new List<Bitmap>();
            textureWidth = heatmap.layout.heatmapWidth;

            numHeatmapTextures = cellsPosition.Count / textureWidth + 1;
            //left side texture
            Bitmap newBitmap = new Bitmap(heatmap.layout.heatmapX, heatmap.layout.bitmapHeight);
            heatmap.layout.textureBitmaps.Add(newBitmap);
            heatmap.layout.textureGraphics.Add(System.Drawing.Graphics.FromImage(newBitmap));
            // middle side (heatmap and grouping/attribute bars)
            for (int i = 0; i < numHeatmapTextures; ++i)
            {
                newBitmap = new Bitmap(textureWidth, heatmap.layout.bitmapHeight);
                heatmap.layout.textureBitmaps.Add(newBitmap);
                heatmap.layout.textureGraphics.Add(System.Drawing.Graphics.FromImage(newBitmap));
            }

            // right side (gene list)
            newBitmap = new Bitmap(heatmap.layout.geneListWidth, heatmap.layout.bitmapHeight);
            heatmap.layout.textureBitmaps.Add(newBitmap);
            heatmap.layout.textureGraphics.Add(System.Drawing.Graphics.FromImage(newBitmap));

            DrawHeatmapSurroundings(heatmap, System.Drawing.Color.FromArgb(0, 0, 0, 0), System.Drawing.Color.White);

            float xcoord = heatmap.layout.heatmapX;
            float ycoord = heatmap.layout.heatmapY;
            float xcoordInc = (float)(heatmap.layout.heatmapWidth * numHeatmapTextures) / heatmap.cells.Length;
            float ycoordInc = (float)heatmap.layout.heatmapHeight / heatmap.genes.Length;
            System.Drawing.SolidBrush[] heatmapBrushes = expressionColors;
            FillRectangle(heatmap, Brushes.Black, heatmap.layout.heatmapX, heatmap.layout.heatmapY, heatmap.layout.heatmapWidth, heatmap.layout.heatmapHeight);
            Tuple<string, float> tuple;
            List<Tuple<string, float>> expressions = new List<Tuple<string, float>>();
            for (int i = 1; i < result.Count;)
            {
                // new gene
                string geneName = "";
                try
                {
                    geneName = ((Tuple<string, float>)result[i]).Item1;
                }
                catch (Exception ex)
                {
                    print($"could not cast to tuple: {result[i]}");
                }


                ycoord = heatmap.layout.heatmapY + genePositions[geneName] * ycoordInc;
                // the arraylist should contain the gene id and that gene's highest expression before all the expressions
                i += 1;
                expressions.Clear();
                while (i < result.Count)
                {
                    tuple = (Tuple<string, float>)result[i];
                    try
                    {
                        tuple = ((Tuple<string, float>)result[i]);
                    }
                    catch (Exception ex)
                    {
                        print($"could not cast to tuple: {result[i]}");
                    }
                    i++;
                    if (geneIds.ContainsKey(tuple.Item1))
                    {
                        break;
                    }
                    expressions.Add(tuple);
                }
                expressions.Sort((Tuple<string, float> t1, Tuple<string, float> t2) => t1.Item2.CompareTo(t2.Item2));
                float binsize = (float)expressions.Count / (CellexalConfig.Config.NumberOfHeatmapColors - 1);
                int expressionIndex = 0;

                for (int j = 0; j < CellexalConfig.Config.NumberOfHeatmapColors; ++j)
                {
                    int nextLimit = (int)(binsize * (j + 1));
                    for (; expressionIndex < nextLimit && expressionIndex < expressions.Count; ++expressionIndex)
                    {
                        expressions[expressionIndex] = new Tuple<string, float>(expressions[expressionIndex].Item1, j);
                    }
                }
                for (int j = 0; j < expressions.Count; ++j)
                {
                    string cellName = expressions[j].Item1;
                    float expression = expressions[j].Item2;
                    xcoord = heatmap.layout.heatmapX + cellsPosition[cellName] * xcoordInc;
                    FillRectangle(heatmap, heatmapBrushes[(int)expression], (int)xcoord, (int)ycoord, (int)xcoordInc, (int)ycoordInc);
                }
            }

            for (int i = 0; i < heatmap.layout.textureGraphics.Count; ++i)
            {
                heatmap.layout.textureGraphics[i].Flush();
                heatmap.layout.textureBitmaps[i].Save(heatmapFilePath + "_" + i + ".png", ImageFormat.Png);
            }
        }

        public void CreateHeatmapPNGScarf(Heatmap heatmap, Dictionary<string, List<Tuple<string, float>>> valueDictionary, Dictionary<string, int> genePositions,
            Dictionary<string, int> cellsPosition, string heatmapFilePath)
        {
            var attributeWidths = heatmap.attributeWidths;
            var groupWidths = heatmap.groupWidths;
            heatmap.layout.textureGraphics = new List<System.Drawing.Graphics>();
            heatmap.layout.textureBitmaps = new List<Bitmap>();
            textureWidth = heatmap.layout.heatmapWidth;

            numHeatmapTextures = cellsPosition.Count / textureWidth + 1;
            //left side texture
            Bitmap newBitmap = new Bitmap(heatmap.layout.heatmapX, heatmap.layout.bitmapHeight);
            heatmap.layout.textureBitmaps.Add(newBitmap);
            heatmap.layout.textureGraphics.Add(System.Drawing.Graphics.FromImage(newBitmap));
            // middle side (heatmap and grouping/attribute bars)
            for (int i = 0; i < numHeatmapTextures; ++i)
            {
                newBitmap = new Bitmap(textureWidth, heatmap.layout.bitmapHeight);
                heatmap.layout.textureBitmaps.Add(newBitmap);
                heatmap.layout.textureGraphics.Add(System.Drawing.Graphics.FromImage(newBitmap));
            }

            // right side (gene list)
            newBitmap = new Bitmap(heatmap.layout.geneListWidth, heatmap.layout.bitmapHeight);
            heatmap.layout.textureBitmaps.Add(newBitmap);
            heatmap.layout.textureGraphics.Add(System.Drawing.Graphics.FromImage(newBitmap));

            DrawHeatmapSurroundings(heatmap, System.Drawing.Color.FromArgb(0, 0, 0, 0), System.Drawing.Color.White);

            float xcoord = heatmap.layout.heatmapX;
            float ycoord = heatmap.layout.heatmapY;
            float xcoordInc = (float)(heatmap.layout.heatmapWidth * numHeatmapTextures) / heatmap.cells.Length;
            float ycoordInc = (float)heatmap.layout.heatmapHeight / heatmap.genes.Length;
            System.Drawing.SolidBrush[] heatmapBrushes = expressionColors;
            FillRectangle(heatmap, Brushes.Black, heatmap.layout.heatmapX, heatmap.layout.heatmapY, heatmap.layout.heatmapWidth, heatmap.layout.heatmapHeight);

            string[] genes = valueDictionary.Keys.ToArray();

            for (int i = 0; i < genes.Length; i++)
            {
                string gene = genes[i];
                List<Tuple<string, float>> expressions = valueDictionary[gene];
                ycoord = heatmap.layout.heatmapY + genePositions[gene] * ycoordInc;

                expressions.Sort((Tuple<string, float> t1, Tuple<string, float> t2) => t1.Item2.CompareTo(t2.Item2));
                float binsize = (float)expressions.Count / (CellexalConfig.Config.NumberOfHeatmapColors - 1);
                int expressionIndex = 0;

                for (int j = 0; j < CellexalConfig.Config.NumberOfHeatmapColors; ++j)
                {
                    int nextLimit = (int)(binsize * (j + 1));
                    for (; expressionIndex < nextLimit && expressionIndex < expressions.Count; ++expressionIndex)
                    {
                        expressions[expressionIndex] = new Tuple<string, float>(expressions[expressionIndex].Item1, j);
                    }
                }

                for (int j = 0; j < expressions.Count; ++j)
                {
                    string cellId = expressions[j].Item1;
                    float expression = expressions[j].Item2;
                    xcoord = heatmap.layout.heatmapX + cellsPosition[cellId] * xcoordInc;
                    FillRectangle(heatmap, heatmapBrushes[(int)expression], (int)xcoord, (int)ycoord, (int)xcoordInc, (int)ycoordInc);
                }
            }

            for (int i = 0; i < heatmap.layout.textureGraphics.Count; ++i)
            {
                heatmap.layout.textureGraphics[i].Flush();
                heatmap.layout.textureBitmaps[i].Save(heatmapFilePath + "_" + i + ".png", ImageFormat.Png);
            }
        }

        /// <summary>
        /// If the heatmap is to be saved for logging purposes (when the user presses save heatmap)
        /// the background color should be white and text colour black for easier reading on a computer screen.
        /// So only the genelist and widths are redrawn.
        /// </summary>
        /// <param name="heatmap">The heatmap to save.</param>
        /// <param name="heatmapFilePath">The filepath where to save the heatmap.</param>
        public void SavePNGtoDisk(Heatmap heatmap, string heatmapFilePath)
        {
            DrawHeatmapSurroundings(heatmap, System.Drawing.Color.White, System.Drawing.Color.Black);
            // sum of all texture widths
            int bitmapWidth = heatmap.layout.heatmapX + heatmap.layout.geneListWidth + heatmap.layout.heatmapWidth * (heatmap.layout.textureGraphics.Count - 2);
            Bitmap bitmap = new Bitmap(bitmapWidth, heatmap.layout.bitmapHeight);
            int xCoord = 0;

            System.Drawing.Graphics combinedGraphics = System.Drawing.Graphics.FromImage(bitmap);
            // left side
            combinedGraphics.DrawImage(heatmap.layout.textureBitmaps[0],
                new Rectangle(0, 0, heatmap.layout.heatmapX, heatmap.layout.bitmapHeight),
                new Rectangle(0, 0, 1, 1),
                GraphicsUnit.Pixel);
            xCoord += heatmap.layout.heatmapX;

            // middle segments
            for (int i = 1; i < heatmap.layout.textureBitmaps.Count - 1; ++i)
            {
                combinedGraphics.DrawImage(heatmap.layout.textureBitmaps[i],
                    new Rectangle(xCoord, 0, heatmap.layout.heatmapWidth, heatmap.layout.bitmapHeight),
                    new Rectangle(0, 0, heatmap.layout.heatmapWidth, heatmap.layout.bitmapHeight),
                    GraphicsUnit.Pixel);
                xCoord += heatmap.layout.heatmapWidth;
            }

            // right side
            combinedGraphics.DrawImage(heatmap.layout.textureBitmaps[heatmap.layout.textureBitmaps.Count - 1],
                new Rectangle(xCoord, 0, heatmap.layout.geneListWidth, heatmap.layout.bitmapHeight),
                new Rectangle(0, 0, heatmap.layout.geneListWidth, heatmap.layout.bitmapHeight),
                GraphicsUnit.Pixel);

            combinedGraphics.Flush();
            bitmap.Save(heatmapFilePath);
            combinedGraphics.Dispose();
        }

        /// <summary>
        /// Helper function to draw the parts of the heatmap that are in common for both logging and in-session heatmaps.
        /// </summary>
        private void DrawHeatmapSurroundings(Heatmap heatmap, System.Drawing.Color backgroundColor, System.Drawing.Color textColor)
        {
            // get the grouping colors
            Dictionary<int, SolidBrush> groupBrushes = new Dictionary<int, SolidBrush>();
            foreach (var entry in heatmap.groupingColors)
            {
                UnityEngine.Color unitycolor = entry.Value;

                groupBrushes[entry.Key] = new SolidBrush(System.Drawing.Color.FromArgb((int)(unitycolor.r * 255),
                    (int)(unitycolor.g * 255), (int)(unitycolor.b * 255)));
            }

            // get the attribute colors
            Dictionary<int, SolidBrush> attributeBrushes = new Dictionary<int, SolidBrush>();
            foreach (var entry in heatmap.attributeColors)
            {
                UnityEngine.Color unitycolor = entry.Value;
                attributeBrushes[entry.Key] = new SolidBrush(System.Drawing.Color.FromArgb((int)(unitycolor.r * 255),
                    (int)(unitycolor.g * 255), (int)(unitycolor.b * 255)));
            }

            // draw a background
            SolidBrush backgroundBrush = new SolidBrush(backgroundColor);
            FillRectangle(heatmap, backgroundBrush, 0, 0, heatmap.layout.heatmapX, heatmap.layout.bitmapHeight);
            FillRectangle(heatmap, backgroundBrush, 0, 0, heatmap.layout.bitmapWidth, heatmap.layout.heatmapY);
            FillRectangle(heatmap, backgroundBrush, heatmap.layout.geneListX, 0, heatmap.layout.geneListWidth, heatmap.layout.bitmapHeight);
            FillRectangle(heatmap, backgroundBrush, 0, (heatmap.layout.bitmapWidth - heatmap.layout.heatmapY), heatmap.layout.bitmapWidth, heatmap.layout.bitmapHeight);

            DrawAttributeBar(heatmap, attributeBrushes);

            DrawGroupingBar(heatmap, groupBrushes);

            DrawGeneList(heatmap, textColor);
        }

        /// <summary>
        /// Draws the attribute bar on a heatmap.
        /// </summary>
        /// <param name="heatmap">The heatmap to draw the attribute bar on.</param>
        /// <param name="attributeBrushes">The color scheme to use.</param>
        private void DrawAttributeBar(Heatmap heatmap, Dictionary<int, SolidBrush> attributeBrushes)
        {
            float xcoord = heatmap.layout.heatmapX;

            for (int i = 0; i < heatmap.attributeWidths.Count; ++i)
            {
                int attributeNbr = heatmap.attributeWidths[i].Item1;
                float attributeWidth = heatmap.attributeWidths[i].Item2 * numHeatmapTextures;
                FillRectangle(heatmap, attributeBrushes[attributeNbr], (int)xcoord, heatmap.layout.attributeBarY, (int)attributeWidth, heatmap.layout.attributeBarHeight);
                xcoord += attributeWidth;
            }
        }

        /// <summary>
        /// Draw the grouping bar on a heatmap.
        /// </summary>
        /// <param name="heatmap">The heatmap to draw the gruping bar on.</param>
        /// <param name="groupBrushes">The coilor scheme to use.</param>
        private void DrawGroupingBar(Heatmap heatmap, Dictionary<int, SolidBrush> groupBrushes)
        {
            float xcoord = heatmap.layout.heatmapX;

            for (int i = 0; i < heatmap.groupWidths.Count; ++i)
            {
                int groupNbr = heatmap.groupWidths[i].Item1;
                float groupWidth = heatmap.groupWidths[i].Item2 * numHeatmapTextures;
                FillRectangle(heatmap, groupBrushes[groupNbr], (int)xcoord, heatmap.layout.groupBarY, (int)groupWidth, heatmap.layout.groupBarHeight);
                xcoord += groupWidth;
            }
        }

        /// <summary>
        /// Draws the gene list on a heatmap.
        /// </summary>
        /// <param name="heatmap">The heatmap to draw the gene list on.</param>
        /// <param name="textColor">The font color.</param>
        private void DrawGeneList(Heatmap heatmap, System.Drawing.Color textColor)
        {
            float ycoord = heatmap.layout.heatmapY - 4;
            float ycoordInc = (float)heatmap.layout.heatmapHeight / heatmap.genes.Length;
            float fontSize;
            if (heatmap.genes.Length > 120)
            {
                fontSize = 8f;
            }
            else
            {
                fontSize = 20f;
            }

            geneFont = new System.Drawing.Font(FontFamily.GenericMonospace, fontSize, System.Drawing.FontStyle.Regular);
            SolidBrush textBrush = new SolidBrush(textColor);
            var geneListTexture = heatmap.layout.textureGraphics[heatmap.layout.textureGraphics.Count - 1];

            for (int i = 0; i < heatmap.genes.Length; ++i)
            {
                string geneName = heatmap.genes[i];

                geneListTexture.DrawString(geneName, geneFont, textBrush, 5, ycoord);
                ycoord += ycoordInc;
            }
        }

        /// <summary>
        /// Draws a rectangle that can span multiple textures.
        /// </summary>
        /// <param name="brush">The desired brush to use when drawing.</param>
        /// <param name="xCoord">The x-coordinate of the rectangles upper left corner.</param>
        /// <param name="yCoord">The y-coordinate of the rectangles upper left corner.</param>
        /// <param name="width">The rectangle's width.</param>
        /// <param name="height">The rectangle's height.</param>
        private void FillRectangle(Heatmap heatmap, Brush brush, int xCoord, int yCoord, int width, int height)
        {
            if (xCoord < heatmap.layout.heatmapX)
            {
                int widthAvailable = textureWidth - xCoord;
                int widthToDraw = Math.Min(width, widthAvailable);
                heatmap.layout.textureGraphics[0].FillRectangle(brush, xCoord, yCoord, widthToDraw, height);
            }

            xCoord -= heatmap.layout.heatmapX;
            int textureIndex = xCoord / textureWidth + 1;
            int texXCoord = xCoord % textureWidth;
            while (width > 0)
            {
                int widthAvailable = textureWidth - texXCoord;
                int widthToDraw = Math.Min(width, widthAvailable);
                heatmap.layout.textureGraphics[textureIndex].FillRectangle(brush, texXCoord, yCoord, widthToDraw, height);
                textureIndex++;
                texXCoord = 0;
                width -= widthToDraw;
            }
        }

        #endregion

        public void AddHeatmapToList(Heatmap heatmap)
        {
            heatmapList.Add(heatmap);
        }
    }
}