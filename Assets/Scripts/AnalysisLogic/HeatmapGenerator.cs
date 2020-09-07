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


        //private GameObject calculatorCluster;
        public SelectionManager selectionManager;
        private ArrayList data;
        private Thread t;
        private SteamVR_Controller.Device device;
        private Vector3 heatmapPosition;
        private List<Heatmap> heatmapList = new List<Heatmap>();
        private string statsMethod;
        private System.Drawing.Font geneFont;
        private int numHeatmapTextures;
        private int textureWidth;

        public UnityEngine.Color HighlightMarkerColor { get; private set; }
        public UnityEngine.Color ConfirmMarkerColor { get; private set; }

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        [ConsoleCommand("heatmapGenerator", folder: "Output\\", aliases: new string[] { "loadheatmapfile", "lhf" })]
        public void LoadHeatmap(string heatmapName)
        {
            statsMethod = CellexalConfig.Config.HeatmapAlgorithm;
            heatmapName = "Output\\" + heatmapName;
            CellexalLog.Log("Loading old heatmap from file " + heatmapName);
            //CellexalEvents.CreatingHeatmap.Invoke();
            //string heatmapName = "heatmap_" +            System.DateTime.Now.ToString("HH-mm-ss");
            // the important parts in the GenerateHeatmapRoutine to            build the heatmap ? !
            var heatmap = Instantiate(heatmapPrefab).GetComponent<Heatmap>();

            heatmap.Init();
            heatmap.transform.parent = transform;
            heatmap.transform.localPosition = heatmapPosition;
            heatmap.selectionNr = selectionNr;
            heatmapList.Add(heatmap);
            heatmap.directory = heatmapName;
            BuildTexture(referenceManager.selectionManager.GetLastSelection(), heatmapName, heatmap);
            heatmap.name = heatmapName; //"heatmap_" + heatmapsCreated;
            heatmap.highlightQuad.GetComponent<Renderer>().material.color = HighlightMarkerColor;
            heatmap.confirmQuad.GetComponent<Renderer>().material.color = ConfirmMarkerColor;
        }

        void Awake()
        {
            t = null;
            heatmapPosition = heatmapPrefab.transform.position;
            selectionManager = referenceManager.selectionManager;
            //status = referenceManager.statusDisplay;
            //statusDisplayHUD = referenceManager.statusDisplayHUD;
            //statusDisplayFar = referenceManager.statusDisplayFar;
            //calculatorCluster = referenceManager.calculatorCluster;
            //calculatorCluster.SetActive(false);
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
            //print(low + " " + mid + " " + high);

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
            //print(midHighDeltaR + " " + midHighDeltaG + " " + midHighDeltaB);

            for (int i = 0; i < numberOfExpressionColors / 2 + 1; ++i)
            {
                float r = low.r * low.r + lowMidDeltaR * i;
                float g = low.g * low.g + lowMidDeltaG * i;
                float b = low.b * low.b + lowMidDeltaB * i;
                if (r < 0) r = 0;
                if (g < 0) g = 0;
                if (b < 0) b = 0;
                expressionColors[i] = new SolidBrush(System.Drawing.Color.FromArgb((int)(Mathf.Sqrt(r) * 255), (int)(Mathf.Sqrt(g) * 255), (int)(Mathf.Sqrt(b) * 255)));
            }
            for (int i = numberOfExpressionColors / 2 + 1, j = 1; i < numberOfExpressionColors; ++i, ++j)
            {
                float r = mid.r * mid.r + midHighDeltaR * j;
                float g = mid.g * mid.g + midHighDeltaG * j;
                float b = mid.b * mid.b + midHighDeltaB * j;
                if (r < 0) r = 0;
                if (g < 0) g = 0;
                if (b < 0) b = 0;
                expressionColors[i] = new SolidBrush(System.Drawing.Color.FromArgb((int)(Mathf.Sqrt(r) * 255), (int)(Mathf.Sqrt(g) * 255), (int)(Mathf.Sqrt(b) * 255)));
            }
            HighlightMarkerColor = CellexalConfig.Config.HeatmapHighlightMarkerColor;
            ConfirmMarkerColor = CellexalConfig.Config.HeatmapConfirmMarkerColor;
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
            StartCoroutine(GenerateHeatmapRoutine(heatmapName));
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
            StartCoroutine(GenerateHeatmapRoutine(heatmapName));
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
        IEnumerator GenerateHeatmapRoutine(string heatmapName)
        {
            GeneratingHeatmaps = true;
            // Show floor pulse
            referenceManager.floor.StartPulse();

            // Check if more than one cell is selected
            while (selectionManager.RObjectUpdating)
            {
                yield return null;
            }
            List<Graph.GraphPoint> selection = selectionManager.GetLastSelection();
            if (selection.Count < 1)
            {
                CellexalLog.Log("can not create heatmap with less than 1 graphpoints, aborting");
                if (!(referenceManager.networkGenerator.GeneratingNetworks && File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.input.R")))
                {
                    referenceManager.floor.StopPulse();
                }
                referenceManager.notificationManager.SpawnNotification("Heatmap generation failed.");
                yield break;
            }
            //string function = "make.cellexalvr.heatmap.list";
            string objectPath = (CellexalUser.UserSpecificFolder + "\\cellexalObj.RData").UnFixFilePath();
            string groupingFilepath = (CellexalUser.UserSpecificFolder + "\\selection" + (selectionManager.fileCreationCtr - 1) + ".txt").UnFixFilePath();
            string topGenesNr = "250";
            string heatmapDirectory = (CellexalUser.UserSpecificFolder + @"\Heatmap").UnFixFilePath();
            string outputFilePath = (heatmapDirectory + @"\\" + heatmapName + ".txt");
            string statsMethod = CellexalConfig.Config.HeatmapAlgorithm;
            string args = CellexalUser.UserSpecificFolder + " " + groupingFilepath + " " + topGenesNr + " " + outputFilePath + " " + statsMethod;
            string rScriptFilePath = (Application.streamingAssetsPath + @"\R\make_heatmap.R").FixFilePath();

            if (!Directory.Exists(heatmapDirectory))
            {
                CellexalLog.Log("Creating directory " + heatmapDirectory.FixFilePath());
                Directory.CreateDirectory(heatmapDirectory);
            }
            bool rServerReady = File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.pid") &&
                    !File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.input.R") &&
                    !File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.input.lock");
            while (!rServerReady || !RScriptRunner.serverIdle)
            {
                rServerReady = File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.pid") &&
                                !File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.input.R") &&
                                !File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.input.lock");
                yield return null;
            }
            //t = new Thread(() => RScriptRunner.RunScript(script));
            t = new Thread(() => RScriptRunner.RunRScript(rScriptFilePath, args));
            t.Start();
            CellexalLog.Log("Running R function " + rScriptFilePath + " with the arguments: " + args);
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            while (t.IsAlive || File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.input.R"))
            {
                yield return null;
            }

            stopwatch.Stop();
            CellexalLog.Log("Heatmap R script finished in " + stopwatch.Elapsed.ToString());

            GeneratingHeatmaps = false;

            var heatmap = Instantiate(heatmapPrefab).GetComponent<Heatmap>();

            heatmap.Init();
            heatmap.transform.parent = transform;
            heatmap.transform.localPosition = heatmapPosition;
            heatmap.selectionNr = selectionNr;
            heatmapList.Add(heatmap);
            heatmap.directory = outputFilePath;
            BuildTexture(selection, outputFilePath, heatmap);
            heatmap.name = heatmapName;
            heatmap.highlightQuad.GetComponent<Renderer>().material.color = HighlightMarkerColor;
            heatmap.confirmQuad.GetComponent<Renderer>().material.color = ConfirmMarkerColor;
            CellexalEvents.CommandFinished.Invoke(true);
        }


        #region BuildTexture Methods
        /// <summary>
        /// Builds the heatmap texture.
        /// </summary>
        /// <param name="selection">An array containing the <see cref="GraphPoint"/> that are in the heatmap</param>
        /// <param name="filepath">A path to the file containing the gene names</param>
        public void BuildTexture(List<Graph.GraphPoint> selection, string filepath, Heatmap heatmap)
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
            heatmap.cells = new Cell[selection.Count];
            heatmap.attributeWidths = new List<Tuple<int, float, int>>();
            heatmap.cellAttributes = new List<Tuple<Cell, int>>();
            heatmap.attributeColors = new Dictionary<int, UnityEngine.Color>();
            heatmap.groupWidths = new List<Tuple<int, float, int>>();
            heatmap.groupingColors = new Dictionary<int, UnityEngine.Color>();
            float cellWidth = (float)heatmap.heatmapWidth / selection.Count;
            int lastGroup = -1;
            int groupWidth = 0;
            heatmap.attributeWidth = 0;
            heatmap.lastAttribute = -1;
            // read the cells and their groups
            for (int i = 0; i < selection.Count; ++i)
            {
                Graph.GraphPoint graphpoint = selection[i];
                int group = graphpoint.Group;
                var cell = referenceManager.cellManager.GetCell(graphpoint.Label);
                heatmap.cells[i] = cell;
                var attributes = cell.Attributes;
                heatmap.AddAttributeWidth(attributes, cellWidth, cell);
                heatmap.groupingColors[group] = graphpoint.GetColor();
                if (lastGroup == -1)
                {
                    lastGroup = group;
                }
                // used for saving the widths of the groups later
                if (group != lastGroup)
                {
                    heatmap.groupWidths.Add(new Tuple<int, float, int>(lastGroup, groupWidth * cellWidth, (int)groupWidth));
                    groupWidth = 0;
                    lastGroup = group;
                }
                groupWidth++;
            }
            // add the last group as well
            heatmap.groupWidths.Add(new Tuple<int, float, int>(lastGroup, groupWidth * cellWidth, groupWidth));
            heatmap.attributeWidths.Add(new Tuple<int, float, int>(heatmap.lastAttribute, heatmap.attributeWidth * cellWidth, heatmap.attributeWidth));
            if (heatmap.genes == null || heatmap.genes.Length == 0)
            {
                if (!File.Exists(filepath))
                {
                    CellexalLog.Log("Failed to create heatmap. R script did not return gene list file " + filepath);
                    CellexalError.SpawnError("Failed to create heatmap", "R script did not return gene list file " + filepath);
                    if (!(referenceManager.networkGenerator.GeneratingNetworks && File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.input.R")))
                    {
                        referenceManager.floor.StopPulse();
                    }
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
            heatmap.orderedByAttribute = false;
            StartCoroutine(BuildTextureCoroutine(heatmap));
        }

        /// <summary>
        /// Builds a heatmap texture based on already known groupwidths and attribute widths.
        /// </summary>
        /// <param name="heatmap">The heatmap to attach the texture to.</param>
        public void BuildTexture(Heatmap heatmap)
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
            StartCoroutine(BuildTextureCoroutine(heatmap));
        }

        /// <summary>
        /// Builds this heatmaps texture using the supplied cells and genes.
        /// </summary>
        /// <param name="newCells">The cells that the heatmap should contain.</param>
        /// <param name="newGenes">The genes that the heatmap should contain.</param>
        /// <param name="newGroupWidths">The grouping information.</param>
        public void BuildTexture(Cell[] newCells, string[] newGenes, List<Tuple<int, float, int>> newGroupWidths, Heatmap heatmap)
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
            StartCoroutine(BuildTextureCoroutine(heatmap));
        }

        /// <summary>
        /// Coroutine that starts a thread and generates the heatmap.
        /// </summary>
        /// <param name="heatmap"></param>
        public IEnumerator BuildTextureCoroutine(Heatmap heatmap)
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

            SQLiter.SQLite db = gameObject.AddComponent<SQLiter.SQLite>();
            db.referenceManager = referenceManager;
            db.InitDatabase(heatmap.directory + ".sqlite3");

            yield return StartCoroutine(db.ValidateDatabaseCoroutine());
            if (!db._databaseOK)
            {
                // something went wrong in the r script
                CellexalError.SpawnError("Could not generate heatmap", "R script did not return a valid gene database file, check R log for more information");
                CellexalLog.Log("Could not generate heatmap, R script did not return a valid gene database file");
                Destroy(gameObject);
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

            ArrayList result = db._result;
            Dictionary<string, string> geneIds = new Dictionary<string, string>(result.Count);
            foreach (Tuple<string, string> t in result)
            {
                // keys are names, values are ids
                geneIds[t.Item1] = t.Item2;
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
            while (db.QueryRunning)
            {
                yield return null;
            }
            db.QueryGenesInCells(heatmap.genes, heatmap.cells.Select((c) => c.Label).ToArray());
            while (db.QueryRunning)
            {
                yield return null;
            }
            result = db._result;
            string heatmapDirectory = Directory.GetCurrentDirectory() + @"\Output\Images";
            if (!Directory.Exists(heatmapDirectory))
            {
                Directory.CreateDirectory(heatmapDirectory);
            }
            string heatmapFilePath = heatmapDirectory + "\\heatmap_temp";


            CellexalLog.Log("Reading " + result.Count + " results from database");
            Thread thread = new Thread(() => CreateHeatmapPNG(heatmap, result, genePositions, cellsPosition, geneIds, heatmapFilePath));
            thread.Start();
            while (thread.IsAlive)
            {
                yield return null;
            }

            // the thread is now done and the heatmap has been painted
            // copy the bitmap data over to a unity texture
            // using a memorystream here seemed like a better alternative but made the standalone crash
            // these yields makes the loading a little bit smoother, but still cuts a few frames.
            List<GameObject> textureGameObjects = heatmap.textureGameObjects;

            if (textureGameObjects != null)
            {
                foreach (GameObject tex in textureGameObjects)
                {
                    Destroy(tex);
                }
            }

            yield return null;

            for (int i = 0; i < heatmap.textureGraphics.Count; ++i)
            {
                var texture = Instantiate(heatmapTexture) as Texture2D;
                yield return null;
                texture.LoadImage(File.ReadAllBytes(heatmapFilePath + "_" + i + ".png"));
                yield return null;
                var newTextureGameObject = Instantiate(heatmapTexturePrefab);
                textureGameObjects.Add(newTextureGameObject);
                newTextureGameObject.GetComponent<Renderer>().material.SetTexture("_MainTex", texture);
                newTextureGameObject.transform.parent = heatmap.transform;
                yield return null;
            }

            // position left and right side textures
            textureGameObjects[0].transform.localPosition = new Vector3(((float)heatmap.heatmapX / heatmap.bitmapWidth / 2f) - 0.5f, 0f, 0f);
            textureGameObjects[0].transform.localScale = new Vector3(((float)heatmap.heatmapX / heatmap.bitmapWidth), 1f, 1f);
            textureGameObjects[0].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            textureGameObjects[textureGameObjects.Count - 1].transform.localPosition = new Vector3((heatmap.geneListX + heatmap.geneListWidth / 2f) / heatmap.bitmapWidth - 0.5f, 0f, 0f);
            textureGameObjects[textureGameObjects.Count - 1].transform.localScale = new Vector3((float)heatmap.geneListWidth / heatmap.bitmapWidth, 1f, 1f);
            textureGameObjects[textureGameObjects.Count - 1].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

            // position middle segment textures
            float textureXStart = (float)heatmap.heatmapX / heatmap.bitmapWidth - 0.5f;
            float textureXInc = (float)(heatmap.bitmapWidth - (heatmap.heatmapX + heatmap.geneListWidth)) / heatmap.bitmapWidth / numHeatmapTextures;
            textureXStart += textureXInc / 2f;
            // since the parent scale is (1, 1, 1) the x-scale (width) of the texture is the same as the distance between two segments
            float textureXScale = textureXInc;
            for (int i = 1; i < textureGameObjects.Count - 1; ++i)
            {
                textureGameObjects[i].transform.localPosition = new Vector3(textureXStart + textureXInc * (i - 1), 0f, 0f);
                textureGameObjects[i].transform.localScale = new Vector3(textureXScale, 1f, 1f);
                textureGameObjects[i].transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

            }
            heatmap.textureGameObjects = textureGameObjects;
            heatmap.GetComponent<Collider>().enabled = true;

            foreach (var button in GetComponentsInChildren<CellexalButton>())
            {
                button.SetButtonActivated(true);
            }

            stopwatch.Stop();
            CellexalLog.Log("Finished building a heatmap texture in " + stopwatch.Elapsed.ToString());
            heatmap.buildingTexture = false;
            heatmap.createAnim = true;

            heatmapsCreated++;
            CellexalEvents.HeatmapCreated.Invoke();
            CellexalEvents.ScriptFinished.Invoke();
            referenceManager.notificationManager.SpawnNotification("Heatmap finished.");
        }

        /// <summary>
        /// Helper method that is run as a thread in <see cref="BuildTextureCoroutine(List{Tuple{int, float, int}}, List{Tuple{int, float, int}}, Heatmap)"/>.
        /// </summary>
        public void CreateHeatmapPNG(Heatmap heatmap, ArrayList result, Dictionary<string, int> genePositions, Dictionary<string, int> cellsPosition,
            Dictionary<string, string> geneIds, string heatmapFilePath)
        {
            var attributeWidths = heatmap.attributeWidths;
            var groupWidths = heatmap.groupWidths;
            heatmap.textureGraphics = new List<System.Drawing.Graphics>();
            heatmap.textureBitmaps = new List<Bitmap>();
            textureWidth = heatmap.heatmapWidth;

            numHeatmapTextures = cellsPosition.Count / textureWidth + 1;
            //left side texture
            Bitmap newBitmap = new Bitmap(heatmap.heatmapX, heatmap.bitmapHeight);
            heatmap.textureBitmaps.Add(newBitmap);
            heatmap.textureGraphics.Add(System.Drawing.Graphics.FromImage(newBitmap));
            // middle side (heatmap and grouping/attribute bars)
            for (int i = 0; i < numHeatmapTextures; ++i)
            {
                newBitmap = new Bitmap(textureWidth, heatmap.bitmapHeight);
                heatmap.textureBitmaps.Add(newBitmap);
                heatmap.textureGraphics.Add(System.Drawing.Graphics.FromImage(newBitmap));
            }
            // right side (gene list)
            newBitmap = new Bitmap(heatmap.geneListWidth, heatmap.bitmapHeight);
            heatmap.textureBitmaps.Add(newBitmap);
            heatmap.textureGraphics.Add(System.Drawing.Graphics.FromImage(newBitmap));

            DrawHeatmapSurroundings(heatmap, System.Drawing.Color.FromArgb(0, 0, 0, 0), System.Drawing.Color.White);

            float xcoord = heatmap.heatmapX;
            float ycoord = heatmap.heatmapY;
            float xcoordInc = (float)(heatmap.heatmapWidth * numHeatmapTextures) / heatmap.cells.Length;
            float ycoordInc = (float)heatmap.heatmapHeight / heatmap.genes.Length;
            System.Drawing.SolidBrush[] heatmapBrushes = expressionColors;
            FillRectangle(heatmap, Brushes.Black, heatmap.heatmapX, heatmap.heatmapY, heatmap.heatmapWidth, heatmap.heatmapHeight);
            Tuple<string, float> tuple;
            List<Tuple<string, float>> expressions = new List<Tuple<string, float>>();
            for (int i = 1; i < result.Count;)
            {
                // new gene
                string genename = ((Tuple<string, float>)result[i]).Item1;

                ycoord = heatmap.heatmapY + genePositions[genename] * ycoordInc;
                // the arraylist should contain the gene id and that gene's highest expression before all the expressions
                i += 1;
                expressions.Clear();
                while (i < result.Count)
                {
                    tuple = (Tuple<string, float>)result[i];
                    i++;
                    if (geneIds.ContainsKey(tuple.Item1))
                        break;
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
                    xcoord = heatmap.heatmapX + cellsPosition[cellName] * xcoordInc;
                    FillRectangle(heatmap, heatmapBrushes[(int)expression], (int)xcoord, (int)ycoord, (int)xcoordInc, (int)ycoordInc);
                }
            }

            for (int i = 0; i < heatmap.textureGraphics.Count; ++i)
            {
                heatmap.textureGraphics[i].Flush();
                heatmap.textureBitmaps[i].Save(heatmapFilePath + "_" + i + ".png", ImageFormat.Png);
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
            int bitmapWidth = heatmap.heatmapX + heatmap.geneListWidth + heatmap.heatmapWidth * (heatmap.textureGraphics.Count - 2);
            Bitmap bitmap = new Bitmap(bitmapWidth, heatmap.bitmapHeight);
            int xCoord = 0;

            System.Drawing.Graphics combinedGraphics = System.Drawing.Graphics.FromImage(bitmap);
            // left side
            combinedGraphics.DrawImage(heatmap.textureBitmaps[0],
                new Rectangle(0, 0, heatmap.heatmapX, heatmap.bitmapHeight),
                new Rectangle(0, 0, 1, 1),
                GraphicsUnit.Pixel);
            xCoord += heatmap.heatmapX;

            // middle segments
            for (int i = 1; i < heatmap.textureBitmaps.Count - 1; ++i)
            {
                combinedGraphics.DrawImage(heatmap.textureBitmaps[i],
                    new Rectangle(xCoord, 0, heatmap.heatmapWidth, heatmap.bitmapHeight),
                    new Rectangle(0, 0, heatmap.heatmapWidth, heatmap.bitmapHeight),
                    GraphicsUnit.Pixel);
                xCoord += heatmap.heatmapWidth;
            }

            // right side
            combinedGraphics.DrawImage(heatmap.textureBitmaps[heatmap.textureBitmaps.Count - 1],
                    new Rectangle(xCoord, 0, heatmap.geneListWidth, heatmap.bitmapHeight),
                    new Rectangle(0, 0, heatmap.geneListWidth, heatmap.bitmapHeight),
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

                groupBrushes[entry.Key] = new SolidBrush(System.Drawing.Color.FromArgb((int)(unitycolor.r * 255), (int)(unitycolor.g * 255), (int)(unitycolor.b * 255)));
            }

            // get the attribute colors
            Dictionary<int, SolidBrush> attributeBrushes = new Dictionary<int, SolidBrush>();
            foreach (var entry in heatmap.attributeColors)
            {
                UnityEngine.Color unitycolor = entry.Value;
                attributeBrushes[entry.Key] = new SolidBrush(System.Drawing.Color.FromArgb((int)(unitycolor.r * 255), (int)(unitycolor.g * 255), (int)(unitycolor.b * 255)));
            }

            // draw a background
            SolidBrush backgroundBrush = new SolidBrush(backgroundColor);
            FillRectangle(heatmap, backgroundBrush, 0, 0, heatmap.heatmapX, heatmap.bitmapHeight);
            FillRectangle(heatmap, backgroundBrush, 0, 0, heatmap.bitmapWidth, heatmap.heatmapY);
            FillRectangle(heatmap, backgroundBrush, heatmap.geneListX, 0, heatmap.geneListWidth, heatmap.bitmapHeight);
            FillRectangle(heatmap, backgroundBrush, 0, (heatmap.bitmapWidth - heatmap.heatmapY), heatmap.bitmapWidth, heatmap.bitmapHeight);

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
            float xcoord = heatmap.heatmapX;

            for (int i = 0; i < heatmap.attributeWidths.Count; ++i)
            {
                int attributeNbr = heatmap.attributeWidths[i].Item1;
                float attributeWidth = heatmap.attributeWidths[i].Item2 * numHeatmapTextures;
                FillRectangle(heatmap, attributeBrushes[attributeNbr], (int)xcoord, heatmap.attributeBarY, (int)attributeWidth, heatmap.attributeBarHeight);
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
            float xcoord = heatmap.heatmapX;

            for (int i = 0; i < heatmap.groupWidths.Count; ++i)
            {
                int groupNbr = heatmap.groupWidths[i].Item1;
                float groupWidth = heatmap.groupWidths[i].Item2 * numHeatmapTextures;
                FillRectangle(heatmap, groupBrushes[groupNbr], (int)xcoord, heatmap.groupBarY, (int)groupWidth, heatmap.groupBarHeight);
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
            float ycoord = heatmap.heatmapY - 4;
            float ycoordInc = (float)heatmap.heatmapHeight / heatmap.genes.Length;
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
            var geneListTexture = heatmap.textureGraphics[heatmap.textureGraphics.Count - 1];

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
            if (xCoord < heatmap.heatmapX)
            {
                int widthAvailable = textureWidth - xCoord;
                int widthToDraw = Math.Min(width, widthAvailable);
                heatmap.textureGraphics[0].FillRectangle(brush, xCoord, yCoord, widthToDraw, height);
            }

            xCoord -= heatmap.heatmapX;
            int textureIndex = xCoord / textureWidth + 1;
            int texXCoord = xCoord % textureWidth;
            while (width > 0)
            {
                int widthAvailable = textureWidth - texXCoord;
                int widthToDraw = Math.Min(width, widthAvailable);
                heatmap.textureGraphics[textureIndex].FillRectangle(brush, texXCoord, yCoord, widthToDraw, height);
                textureIndex++;
                texXCoord = 0;
                width -= widthToDraw;
            }

        }
        #endregion

        public void AddHeatmapToList(Heatmap heatmap)
        {
            heatmapList.Add(heatmap);
            heatmap.selectionNr = selectionNr;
        }

    }
}
