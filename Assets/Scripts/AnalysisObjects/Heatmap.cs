using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using VRTK;
using System.Collections;
using System.Drawing.Imaging;
using System.Threading;
using TMPro;
using System.Drawing;
using CellexalVR.Menu.Buttons.Heatmap;
using CellexalVR.Menu.Buttons.Report;
using CellexalVR.General;
using CellexalVR.AnalysisLogic;
using CellexalVR.Interaction;
using CellexalVR.Menu.Buttons;
using CellexalVR.Extensions;
using System.Linq;

namespace CellexalVR.AnalysisObjects
{
    /// <summary>
    /// This class represents a heatmap. Contains methods for calling r-script, building texture and interaction methods etc.
    /// </summary>
    public class Heatmap : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public Texture texture;
        public TextMeshPro infoText;
        public TextMeshPro statusText;
        public SaveHeatmapButton saveImageButton;
        public GOanalysisButton goAnalysisButton;
        public GameObject highlightQuad;
        public GameObject highlightGeneQuad;
        public GameObject confirmQuad;
        public GameObject movingQuadX;
        public GameObject movingQuadY;
        public int selectionNr;
        public TextMeshPro enlargedGeneText;
        public TextMeshPro highlightGeneText;
        public bool removable;
        public string directory;

        private GraphManager graphManager;
        private CellManager cellManager;
        private SteamVR_Controller.Device device;
        private GameObject deleteTool;
        private SteamVR_TrackedObject rightController;
        private Transform raycastingSource;
        private GameManager gameManager;
        private TextMeshPro highlightInfoText;
        private ControllerModelSwitcher controllerModelSwitcher;
        private HeatmapGenerator heatmapGenerator;

        private Bitmap bitmap;
        private string[] cells;
        /// <summary>
        /// Item1: group number, Item2: group width in coordinates, Item3: number of cells in the group
        /// </summary>
        private List<Tuple<int, float, int>> groupWidths;
        private List<Tuple<int, float, int>> attributeWidths;
        private Dictionary<int, UnityEngine.Color> groupingColors;
        private Dictionary<int, UnityEngine.Color> attributeColors;
        private string[] genes;
        private int bitmapWidth = 4096;
        private int bitmapHeight = 4096;
        private int heatmapX = 250;
        private int heatmapY = 250;
        private int heatmapWidth = 3596;
        private int heatmapHeight = 3596;
        private int geneListX = 3846;
        private int geneListWidth = 250;
        private int attributeBarY = 0;
        private int groupBarY = 120;
        private int groupBarHeight = 100;
        private int attributeBarHeight = 100;
        private System.Drawing.Font geneFont;
        private int numberOfExpressionColors;
        private SolidBrush[] heatmapBrushes;
        private bool buildingTexture = false;
        private LineRenderer lineRenderer;

        private int selectionStartX;
        private int selectionStartY;
        private bool selecting = false;
        private bool movingSelection = false;
        private int layerMask;

        // For creation animation
        private bool createAnim = false;
        private float targetScale;
        private float speed;
        private float scaleSpeed;
        private Vector3 target;
        // Minimizing
        private Vector3 originalPos;
        private Quaternion originalRot;
        private Vector3 originalScale;
        private float targetMinScale;
        private bool minimize;
        private bool maximize;

        private bool highlight;
        private float highlightTime = 0;

        // these are numbers ranging [0, groupWidths.Length)
        private int selectedGroupLeft;
        private int selectedGroupRight;
        // these are numbers ranging [0, genes.Length)
        private int selectedGeneTop;
        private int selectedGeneBottom;
        // these are the actual coordinates and size of the box
        private float selectedBoxX;
        private float selectedBoxY;
        private float selectedBoxWidth;
        private float selectedBoxHeight;
        // number of heatmaps created from this heatmap
        private int heatmapsCreated = 0;
        //private bool heatmapSaved; 

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        void Start()
        {
            //Init();
            targetScale = 2f;
            speed = 2f;
            scaleSpeed = 3f;
            transform.localScale = new Vector3(0f, 0f, 0f);
            target = new Vector3(1.4f, 1.2f, 0.05f);
            originalPos = originalScale = new Vector3();
            originalRot = new Quaternion();
            layerMask = 1 << LayerMask.NameToLayer("GraphLayer");

        }

        public void Init()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            //referenceManager = heatmapGenerator.referenceManager;
            GetComponent<HeatmapGrab>().referenceManager = referenceManager;
            if (CrossSceneInformation.Normal)
            {
                rightController = referenceManager.rightController;
                raycastingSource = rightController.transform;
                controllerModelSwitcher = referenceManager.controllerModelSwitcher;
                deleteTool = referenceManager.deleteTool;
            }
            graphManager = referenceManager.graphManager;
            cellManager = referenceManager.cellManager;
            gameManager = referenceManager.gameManager;
            highlightQuad.SetActive(false);
            highlightGeneQuad.SetActive(false);
            confirmQuad.SetActive(false);
            movingQuadX.SetActive(false);
            movingQuadY.SetActive(false);
            highlightInfoText = highlightQuad.GetComponentInChildren<TextMeshPro>();
            geneFont = new System.Drawing.Font(FontFamily.GenericMonospace, 14f, System.Drawing.FontStyle.Bold);

            numberOfExpressionColors = CellexalConfig.Config.NumberOfHeatmapColors;
            heatmapGenerator = referenceManager.heatmapGenerator;
            highlightQuad.GetComponent<Renderer>().material.color = heatmapGenerator.HighlightMarkerColor;
            confirmQuad.GetComponent<Renderer>().material.color = heatmapGenerator.ConfirmMarkerColor;
            foreach (CellexalButton b in GetComponentsInChildren<CellexalButton>())
            {
                b.referenceManager = referenceManager;
            }

            //lineRenderer = highlightGeneQuad.AddComponent<LineRenderer>();
            //lineRenderer.useWorldSpace = false;
            //lineRenderer.startWidth = lineRenderer.endWidth = 0.005f;
            //lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            //lineRenderer.startColor = new UnityEngine.Color(29, 146, 178); // same as outline on heatmap
            //lineRenderer.endColor = new UnityEngine.Color(29, 146, 178);
            //lineRenderer.SetPosition(0, new Vector3(0.5f, 0, 0));
            //lineRenderer.SetPosition(1, new Vector3(5f, 0, 0));

        }

        void Update()
        {
            if (device == null && CrossSceneInformation.Normal)
            {
                device = SteamVR_Controller.Input((int)rightController.index);
            }
            if (createAnim)
            {
                CreateHeatmapAnimation();
            }
            if (minimize)
            {
                Minimize();
            }
            if (maximize)
            {
                Maximize();
            }
            if (highlight)
            {
                highlightTime += Time.deltaTime;
                if (highlightTime > 6f)
                {
                    highlight = false;
                    highlightTime = 0;
                    highlightGeneQuad.SetActive(false);
                    highlightGeneText.text = "";
                }
            }


            if (GetComponent<VRTK_InteractableObject>().IsGrabbed())
            {
                gameManager.InformMoveHeatmap(name, transform.position, transform.rotation, transform.localScale);
            }
            if (CrossSceneInformation.Normal || CrossSceneInformation.Tutorial)
            {
                bool correctModel = controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.TwoLasers
                                    || controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.Keyboard
                                    || controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.WebBrowser;
                if (correctModel)
                    HeatmapRaycast();
            }

        }

        void HeatmapRaycast()
        {
            raycastingSource = referenceManager.rightLaser.transform;
            //Ray ray = new Ray(raycastingSource.position, raycastingSource.forward);
            RaycastHit hit;
            Physics.Raycast(raycastingSource.position, raycastingSource.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask);
            if (hit.collider && hit.transform == transform)
            {
                int hitx = (int)(hit.textureCoord.x * bitmapWidth);
                int hity = (int)(hit.textureCoord.y * bitmapHeight);
                if (CoordinatesInsideRect(hitx, hity, geneListX, heatmapY, geneListWidth, heatmapHeight))
                {
                    // if we hit the list of genes
                    int geneHit = HandleHitGeneList(hity);

                    if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                    {
                        gameManager.InformColorGraphsByGene(genes[geneHit]);
                        referenceManager.cellManager.ColorGraphsByGene(genes[geneHit], graphManager.GeneExpressionColoringMethod);
                    }
                }
                else if (CoordinatesInsideRect(hitx, bitmapHeight - hity, heatmapX, groupBarY, heatmapWidth, groupBarHeight))
                {
                    // if we hit the grouping bar
                    HandleHitGroupingBar(hitx);
                    enlargedGeneText.text = "";
                }
                else if (CoordinatesInsideRect(hitx, bitmapHeight - hity, heatmapX, heatmapY, heatmapWidth, heatmapHeight))
                {
                    // if we hit the actual heatmap
                    if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                    {
                        gameManager.InformHandlePressDown(name, hitx, hity);
                        HandlePressDown(hitx, hity);
                    }

                    if (device.GetPress(SteamVR_Controller.ButtonMask.Trigger) && selecting)
                    {
                        // called when choosing a box selection
                        gameManager.InformHandleBoxSelection(name, hitx, hity, selectionStartX, selectionStartY);
                        HandleBoxSelection(hitx, hity, selectionStartX, selectionStartY);
                    }
                    else if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger) && selecting)
                    {
                        // called when letting go of the trigger to finalize a box selection
                        gameManager.InformConfirmSelection(name, hitx, hity, selectionStartX, selectionStartY);
                        ConfirmSelection(hitx, hity, selectionStartX, selectionStartY);
                    }
                    else if (device.GetPress(SteamVR_Controller.ButtonMask.Trigger) && movingSelection)
                    {
                        // called when moving a selection
                        gameManager.InformHandleMovingSelection(name, hitx, hity);
                        HandleMovingSelection(hitx, hity);
                    }
                    else if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger) && movingSelection)
                    {
                        // called when letting go of the trigger to move the selection
                        gameManager.InformMoveSelection(name, hitx, hity, selectedGroupLeft, selectedGroupRight, selectedGeneTop, selectedGeneBottom);
                        MoveSelection(hitx, hity, selectedGroupLeft, selectedGroupRight, selectedGeneTop, selectedGeneBottom);

                    }
                    else
                    {
                        // handle when the raycast just hits the heatmap
                        gameManager.InformHandleHitHeatmap(name, hitx, hity);
                        HandleHitHeatmap(hitx, hity);
                    }
                    enlargedGeneText.gameObject.SetActive(false);
                }
                else
                {
                    // if we hit the heatmap but not any area of interest, like the borders or any space in between
                    gameManager.InformResetHeatmapHighlight(name);
                    ResetHeatmapHighlight();
                    enlargedGeneText.gameObject.SetActive(false);
                }
            }
            else
            {
                // if we don't hit the heatmap at all
                gameManager.InformResetHeatmapHighlight(name);
                ResetHeatmapHighlight();
            }
            if (device.GetPressUp(SteamVR_Controller.ButtonMask.Trigger))
            {
                // if the raycast leaves the heatmap and the user lets go of the trigger
                gameManager.InformResetSelecting(name);
                ResetSelecting();
            }
        }

        /// <summary>
        /// Builds the heatmap texture.
        /// </summary>
        /// <param name="selection">An array containing the <see cref="GraphPoint"/> that are in the heatmap</param>
        /// <param name="filepath">A path to the file containing the gene names</param>
        public void BuildTexture(List<Graph.GraphPoint> selection, string filepath)
        {

            if (buildingTexture)
            {
                CellexalLog.Log("WARNING: Not building heatmap texture because it is already building");
                return;
            }
            gameObject.SetActive(true);
            GetComponent<Collider>().enabled = false;

            cells = new string[selection.Count];
            groupWidths = new List<Tuple<int, float, int>>();
            attributeWidths = new List<Tuple<int, float, int>>();
            groupingColors = new Dictionary<int, UnityEngine.Color>();
            attributeColors = new Dictionary<int, UnityEngine.Color>();
            float cellWidth = (float)heatmapWidth / selection.Count;
            int lastGroup = -1;
            int lastAttribute = -1;
            int groupWidth = 0;
            int attributeWidth = 0;
            // read the cells and their groups
            for (int i = 0; i < selection.Count; ++i)
            {
                Graph.GraphPoint graphpoint = selection[i];
                int group = graphpoint.Group;
                groupingColors[group] = graphpoint.GetColor();
                //var attributes = cellManager.GetCell(graphpoint.Label).Attributes;
                //if (attributes.Count > 0)
                //{
                //    var attribute = attributes.First();
                //    attributeColors[attribute.Value] = referenceManager.selectionManager.GetColor(attribute.Value);
                //    if (lastAttribute == -1)
                //    {
                //        lastAttribute = attribute.Value;
                //    }

                //    if (attribute.Value != lastAttribute)
                //    {
                //        attributeWidths.Add(new Tuple<int, float, int>(lastAttribute, attributeWidth * cellWidth, (int)attributeWidth));
                //        attributeWidth = 0;
                //        lastAttribute = attribute.Value;
                //    }
                //    //print("key : " + attributes.First().Key + ", value : " + attributes.First().Value);
                //}
                cells[i] = graphpoint.Label;
                if (lastGroup == -1)
                {
                    lastGroup = group;
                }

                // used for saving the widths of the groups later
                if (group != lastGroup)
                {
                    groupWidths.Add(new Tuple<int, float, int>(lastGroup, groupWidth * cellWidth, (int)groupWidth));
                    groupWidth = 0;
                    lastGroup = group;
                }
                groupWidth++;
                attributeWidth++;
            }
            // add the last group as well
            groupWidths.Add(new Tuple<int, float, int>(lastGroup, groupWidth * cellWidth, groupWidth));
            attributeWidths.Add(new Tuple<int, float, int>(lastAttribute, attributeWidth * cellWidth, attributeWidth));
            if (genes == null || genes.Length == 0)
            {
                try
                {
                    StreamReader streamReader = new StreamReader(filepath);
                    int numberOfGenes = int.Parse(streamReader.ReadLine());
                    genes = new string[numberOfGenes];
                    int i = 0;
                    while (!streamReader.EndOfStream)
                    {
                        genes[i] = streamReader.ReadLine();
                        i++;
                    }
                    streamReader.Close();
                }
                catch (FileNotFoundException)
                {
                    Debug.Log("File - " + filepath + " - not found.");
                    CellexalLog.Log("File - " + filepath + " - not found.");
                    CellexalError.SpawnError("Failed to create heatmap", "Read full stacktrace in cellexal log");
                    if (!referenceManager.networkGenerator.GeneratingNetworks)
                        referenceManager.calculatorCluster.SetActive(false);
                }

            }

            try
            {
                StartCoroutine(BuildTextureCoroutine(groupWidths));
            }
            catch (Exception e)
            {
                CellexalLog.Log("Failed to create heatmap - " + e.StackTrace);
                CellexalError.SpawnError("Failed to create heatmap", "Read full stacktrace in cellexal log");
            }
        }

        private void BuildTexture(List<Tuple<int, float, int>> groupWidths)
        {
            if (buildingTexture)
            {
                CellexalLog.Log("WARNING: Not building heatmap texture because it is already building");
                return;
            }
            GetComponent<Collider>().enabled = false;
            // merge groups
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
            StartCoroutine(BuildTextureCoroutine(groupWidths));
            //try
            //{
            //}
            //catch (Exception e)
            //{
            //    CellexalLog.Log("Failed to create heatmap - " + e.StackTrace);
            //    CellexalError.SpawnError("Failed to create heatmap", "Read full stacktrace in cellexal log");
            //}
        }

        /// <summary>
        /// Builds this heatmaps texture using the supplied cells and genes.
        /// </summary>
        /// <param name="newCells">The cells that the heatmap should contain.</param>
        /// <param name="newGenes">The genes that the heatmap should contain.</param>
        /// <param name="newGroupWidths">The grouping information.</param>
        private void BuildTexture(string[] newCells, string[] newGenes, List<Tuple<int, float, int>> newGroupWidths)
        {
            if (buildingTexture)
            {
                CellexalLog.Log("WARNING: Not building heatmap texture because it is already building");
                return;
            }
            cells = newCells;
            genes = newGenes;
            groupWidths = newGroupWidths;
            StartCoroutine(BuildTextureCoroutine(groupWidths));
            //try
            //{
            //}
            //catch (Exception e)
            //{
            //    CellexalLog.Log("Failed to create heatmap - " + e.StackTrace);
            //    CellexalError.SpawnError("Failed to create heatmap", "Read full stacktrace in cellexal log");
            //}
        }

        private IEnumerator BuildTextureCoroutine(List<Tuple<int, float, int>> groupWidths)
        {
            buildingTexture = true;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            CellexalLog.Log("Started building a heatmap texture");

            foreach (var button in GetComponentsInChildren<CellexalButton>())
            {
                button.SetButtonActivated(false);
            }

            //SQLiter.SQLite database = referenceManager.database;
            //SQLiter.SQLite db = Instantiate(SQLiter.SQLite);
            //{
            //    referenceManager = referenceManager
            //};
            SQLiter.SQLite db = gameObject.AddComponent<SQLiter.SQLite>();
            db.referenceManager = referenceManager;
            db.InitDatabase(this.directory + ".sqlite3");

            bitmap = new Bitmap(bitmapWidth, bitmapHeight);
            System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(bitmap);

            // get the grouping colors
            Dictionary<int, SolidBrush> groupBrushes = new Dictionary<int, SolidBrush>();
            foreach (var entry in groupingColors)
            {
                UnityEngine.Color unitycolor = entry.Value;

                groupBrushes[entry.Key] = new SolidBrush(System.Drawing.Color.FromArgb((int)(unitycolor.r * 255), (int)(unitycolor.g * 255), (int)(unitycolor.b * 255)));
            }

            //Dictionary<int, SolidBrush> attributeBrushes = new Dictionary<int, SolidBrush>();
            //foreach (var entry in attributeColors)
            //{
            //    UnityEngine.Color unitycolor = entry.Value;
            //    attributeBrushes[entry.Key] = new SolidBrush(System.Drawing.Color.FromArgb((int)(unitycolor.r * 255), (int)(unitycolor.g * 255), (int)(unitycolor.b * 255)));
            //}
            // draw a white background
            graphics.Clear(System.Drawing.Color.FromArgb(0, 0, 0, 0));

            float xcoord = heatmapX;
            float ycoord = heatmapY;
            float xcoordInc = (float)heatmapWidth / cells.Length;
            float ycoordInc = (float)heatmapHeight / genes.Length;
            //float cellwidth = (float)heatmapWidth / attributeColors.Count;
            //// draw the grouping bar
            //for (int i = 0; i < attributeBrushes.Count; ++i)
            //{
            //    int attributeNbr = attributeWidths[i].Item1;
            //    float attributeWidth = attributeWidths[i].Item2;
            //    graphics.FillRectangle(attributeBrushes[attributeNbr], xcoord, attributeBarY, attributeWidth, attributeBarHeight);
            //    xcoord += attributeWidth;
            //}

            //xcoord = heatmapX;
            //ycoord = heatmapY;
            // draw the grouping bar
            for (int i = 0; i < groupWidths.Count; ++i)
            {
                int groupNbr = groupWidths[i].Item1;
                float groupWidth = groupWidths[i].Item2;
                graphics.FillRectangle(groupBrushes[groupNbr], xcoord, groupBarY, groupWidth, groupBarHeight);
                xcoord += groupWidth;
            }
            //xcoord = heatmapX;

            while (db.QueryRunning)
            {
                yield return null;
            }
            //db.QueryGenesIds(genes);
            //while (db.QueryRunning)
            //{
            //    yield return null;
            //}

            //ArrayList result = db._result;
            //Dictionary<string, string> geneIds = new Dictionary<string, string>(result.Count);
            //foreach (Tuple<string, string> t in result)
            //{
            //    // ids are keys, names are values
            //    geneIds[t.Item2] = t.Item1;
            //}

            Dictionary<string, int> genePositions = new Dictionary<string, int>(genes.Length);
            for (int i = 0; i < genes.Length; ++i)
            {
                // gene names are keys, positions are values
                genePositions[genes[i]] = i;
            }

            Dictionary<string, int> cellsPosition = new Dictionary<string, int>(cells.Length);

            for (int i = 0; i < cells.Length; ++i)
            {
                cellsPosition[cells[i]] = i;
            }
            while (db.QueryRunning)
            {
                yield return null;
            }
            db.QueryGenesInCells(genes, cells);
            while (db.QueryRunning)
            {
                yield return null;
            }
            ArrayList result = db._result;

            CellexalLog.Log("Reading " + result.Count + " results from database");
            //Thread thread = new Thread(() =>
            //{
            System.Drawing.SolidBrush[] heatmapBrushes = heatmapGenerator.expressionColors;
            float lowestExpression = 0;
            float highestExpression = 0;
            graphics.FillRectangle(Brushes.Black, heatmapX, heatmapY, heatmapWidth, heatmapHeight);
            int genescount = 0;
            //print(result.Count);
            for (int i = 0; i < result.Count; ++i)
            {
                // the arraylist should contain the gene id and that gene's highest expression before all the expressions
                Tuple<string, float> tuple = (Tuple<string, float>)result[i];
                //if (geneIds.ContainsKey(tuple.Item1))
                //{
                // new gene
                lowestExpression = tuple.Item2;
                i++;
                tuple = (Tuple<string, float>)result[i];
                highestExpression = tuple.Item2;
                ycoord = heatmapY + genePositions[tuple.Item1] * ycoordInc;
                genescount++;
                i++;
                //}
                //else
                //{
                List<Tuple<string, float>> expressions = new List<Tuple<string, float>>();
                tuple = (Tuple<string, float>)result[i];
                do
                {
                    expressions.Add(tuple);
                    i++;
                    if (i >= result.Count)
                        break;
                    tuple = (Tuple<string, float>)result[i];
                }
                while (!genePositions.ContainsKey(tuple.Item1));
                i--;
                expressions.Sort((Tuple<string, float> t1, Tuple<string, float> t2) => ((int)((t2.Item2 - t1.Item2) * 10000)));
                float binsize = (float)expressions.Count / CellexalConfig.Config.NumberOfHeatmapColors;
                for (int j = 0, k = 0; j < CellexalConfig.Config.NumberOfHeatmapColors; ++j)
                {
                    int nextLimit = (int)(binsize * j);
                    for (; k < nextLimit; ++k)
                    {
                        expressions[k] = new Tuple<string, float>(expressions[k].Item1, j);
                    }
                }
                //print(expressions.Count);
                for (int j = 0; j < expressions.Count; ++j)
                {
                    string cellName = expressions[j].Item1;
                    float expression = expressions[j].Item2;
                    xcoord = heatmapX + cellsPosition[cellName] * xcoordInc;
                    graphics.FillRectangle(heatmapBrushes[(int)expression], xcoord, ycoord, xcoordInc, ycoordInc);
                }

                //if (expression == highestExpression)
                //{
                //    graphics.FillRectangle(heatmapBrushes[heatmapBrushes.Length - 1], xcoord, ycoord, xcoordInc, ycoordInc);
                //}
                //else
                //{
                //graphics.FillRectangle(heatmapBrushes[(int)((expression - lowestExpression) / (highestExpression - lowestExpression) * numberOfExpressionColors)], xcoord, ycoord, xcoordInc, ycoordInc);
                //}
                //}
            }
            ycoord = heatmapY;
            // draw all the gene names
            for (int i = 0; i < genes.Length; ++i)
            {
                string geneName = genes[i];
                graphics.DrawString(geneName, geneFont, Brushes.White, geneListX, ycoord);
                //graphics.DrawString(geneName, geneFont, SystemBrushes.Menu, geneListX, ycoord);
                ycoord += ycoordInc;
            }
            //});

            //thread.Start();
            //while (thread.IsAlive)
            //{
            //    yield return null;
            //}

            //using (MemoryStream memoryStream = new MemoryStream())
            //{
            //    bitmap.Save(memoryStream, ImageFormat.Png);
            //    yield return null;
            //    byte[] bytes = memoryStream.ToArray();
            //    yield return null;
            //    var texture = new Texture2D(bitmapWidth, bitmapHeight);
            //    yield return null;
            //    texture.LoadImage(bytes);
            //    yield return null;
            //    GetComponent<Renderer>().material.SetTexture("_MainTex", texture);
            //    yield return null;
            //}


            // the thread is now done and the heatmap has been painted
            // copy the bitmap data over to a unity texture
            // using a memorystream here seemed like a better alternative but made the standalone crash
            string heatmapDirectory = Directory.GetCurrentDirectory() + @"\Output\Images";
            if (!Directory.Exists(heatmapDirectory))
            {
                Directory.CreateDirectory(heatmapDirectory);
            }
            string heatmapFilePath = heatmapDirectory + "\\heatmap_temp.png";
            bitmap.Save(heatmapFilePath, ImageFormat.Png);
            // these yields makes the loading a little bit smoother, but still cuts a few frames.
            var texture = new Texture2D(4096, 4096);
            yield return null;
            texture.LoadImage(File.ReadAllBytes(heatmapFilePath));
            this.texture = texture;
            yield return null;
            GetComponent<Renderer>().material.SetTexture("_MainTex", texture);
            yield return null;

            GetComponent<Collider>().enabled = true;
            graphics.Dispose();

            foreach (var button in GetComponentsInChildren<CellexalButton>())
            {
                button.SetButtonActivated(true);
            }

            stopwatch.Stop();
            CellexalLog.Log("Finished building a heatmap texture in " + stopwatch.Elapsed.ToString());
            buildingTexture = false;
            createAnim = true;

            CellexalEvents.HeatmapCreated.Invoke();
            if (!referenceManager.networkGenerator.GeneratingNetworks)
                referenceManager.calculatorCluster.SetActive(false);

            referenceManager.notificationManager.SpawnNotification("Heatmap finished.");
        }


        private void CreateHeatmapAnimation()
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, target, step);
            transform.localScale += Vector3.one * Time.deltaTime * scaleSpeed;
            if (transform.localScale.x >= targetScale)
            {
                createAnim = false;
            }
        }

        internal void HideHeatmap()
        {
            originalPos = transform.position;
            originalRot = transform.localRotation;
            originalScale = transform.localScale;
            foreach (Collider c in GetComponentsInChildren<Collider>())
            {
                c.enabled = false;
            }
            minimize = true;
        }

        private void Minimize()
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, referenceManager.minimizedObjectHandler.transform.position, step);
            transform.localScale -= Vector3.one * Time.deltaTime * scaleSpeed;
            transform.Rotate(Vector3.one * Time.deltaTime * 50);
            if (transform.localScale.x <= targetMinScale)
            {
                minimize = false;
                foreach (Renderer r in GetComponentsInChildren<Renderer>())
                    r.enabled = false;
                GetComponent<Renderer>().enabled = false;
                referenceManager.minimizeTool.GetComponent<Light>().range = 0.04f;
                referenceManager.minimizeTool.GetComponent<Light>().intensity = 0.8f;
            }
        }

        internal void ShowHeatmap()
        {
            transform.position = referenceManager.minimizedObjectHandler.transform.position;
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                r.enabled = true;
            GetComponent<Renderer>().enabled = true;
            maximize = true;
        }

        private void Maximize()
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, originalPos, step);
            transform.localScale += Vector3.one * Time.deltaTime * scaleSpeed;
            transform.Rotate(Vector3.one * Time.deltaTime * -50);
            if (transform.localScale.x >= originalScale.x)
            {
                transform.localScale = originalScale;
                transform.position = originalPos;
                transform.rotation = originalRot;
                maximize = false;
                foreach (Collider c in GetComponentsInChildren<Collider>())
                    c.enabled = true;
            }
        }



        public void HandlePressDown(int hitx, int hity)
        {
            if (CoordinatesInsideRect(hitx, bitmapHeight - hity, (int)selectedBoxX, (int)selectedBoxY, (int)selectedBoxWidth, (int)selectedBoxHeight))
            {
                // if we hit a confirmed selection
                movingSelection = true;
            }
            else
            {
                // if we hit something else
                selecting = true;
                selectionStartX = hitx;
                selectionStartY = hity;
            }
        }

        public void ResetHeatmapHighlight()
        {
            highlightQuad.SetActive(false);
            highlightInfoText.text = "";
        }

        public void ResetSelecting()
        {
            selecting = false;
            movingSelection = false;
        }

        /// <summary>
        /// Handles the highlighting when the raycast hits the heatmap
        /// </summary>
        /// <param name="hitx"> The x coordinate of the hit. Measured in pixels of the texture.</param>
        /// <param name="hity">The x coordinate if the hit. Meaured in pixels of the texture.</param>
        public void HandleHitHeatmap(int hitx, int hity)
        {
            // get this groups width and xcoordinate
            float groupX, groupWidth;
            int group;
            FindGroupInfo(hitx, out groupX, out groupWidth, out group);

            int geneHit = (int)((float)((bitmapHeight - hity) - heatmapY) / heatmapHeight * genes.Length);
            float highlightMarkerWidth = groupWidth / bitmapWidth;
            float highlightMarkerHeight = ((float)heatmapHeight / bitmapHeight) / genes.Length;
            float highlightMarkerX = groupX / bitmapWidth + highlightMarkerWidth / 2 - 0.5f;
            float highlightMarkerY = -(float)heatmapY / bitmapHeight - geneHit * (highlightMarkerHeight) - highlightMarkerHeight / 2 + 0.5f;

            highlightQuad.transform.localPosition = new Vector3(highlightMarkerX, highlightMarkerY, -0.001f);
            highlightQuad.transform.localScale = new Vector3(highlightMarkerWidth, highlightMarkerHeight, 1f);
            highlightQuad.SetActive(true);
            highlightInfoText.text = "Group: " + group + "\nGene: " + genes[geneHit];

            // the smaller the highlight quad becomes, the larger the text has to become
            highlightInfoText.transform.localScale = new Vector3(0.003f / highlightMarkerWidth, 0.003f / highlightMarkerHeight, 0.003f);
        }

        /// <summary>
        /// Finds out some info about what group is at a x coordinate.
        /// </summary>
        /// <param name="hitx">The x coordinate that the raycast hit.</param>
        /// <param name="groupX">The leftmost x coordinate of the group that was hit.</param>
        /// <param name="groupWidth">The width of the group, measured in pixels.</param>
        /// <param name="group">The number (color) of the group.</param>
        private void FindGroupInfo(int hitx, out float groupX, out float groupWidth, out int group)
        {
            groupX = heatmapX;
            groupWidth = 0;
            group = 0;
            for (int i = 0; i < groupWidths.Count; ++i)
            {
                if (groupX + groupWidths[i].Item2 > hitx)
                {
                    group = groupWidths[i].Item1;
                    groupWidth = groupWidths[i].Item2;
                    break;
                }
                groupX += groupWidths[i].Item2;
            }
        }

        /// <summary>
        /// Handles the highlighting when the raycast hits the grouping bar.
        /// The grouping bar is only 1 item tall and thus we do not care about the y coorindate.
        /// </summary>
        /// <param name="hitx"> The xcoordinate of the hit.</param>
        public void HandleHitGroupingBar(int hitx)
        {
            // get this groups width and xcoordinate
            float groupX, groupWidth;
            int group;
            FindGroupInfo(hitx, out groupX, out groupWidth, out group);

            float highlightMarkerWidth = groupWidth / bitmapWidth;
            float highlightMarkerHeight = ((float)groupBarHeight / bitmapHeight);
            float highlightMarkerX = groupX / bitmapWidth + highlightMarkerWidth / 2 - 0.5f;
            float highlightMarkerY = -(float)groupBarY / bitmapHeight - highlightMarkerHeight / 2 + 0.5f;

            highlightQuad.transform.localPosition = new Vector3(highlightMarkerX, highlightMarkerY, -0.001f);
            highlightQuad.transform.localScale = new Vector3(highlightMarkerWidth, highlightMarkerHeight, 1f);
            highlightQuad.SetActive(true);
            highlightInfoText.text = "";
        }

        /// <summary>
        /// Handles the highlighting of the gene list.
        /// The gene list is only 1 item wide and thus we do not care about the xcoordinate.
        /// </summary>
        /// <param name="hity">The y coordinate of the hit.</param>
        /// <returns>An index of the gene that was hit.</returns>
        public int HandleHitGeneList(int hity)
        {
            int geneHit = (int)((float)((bitmapHeight - hity) - heatmapY) / heatmapHeight * genes.Length);

            float highlightMarkerWidth = (float)geneListWidth / bitmapWidth;
            float highlightMarkerHeight = ((float)heatmapHeight / bitmapHeight) / genes.Length;
            float highlightMarkerX = (float)geneListX / bitmapWidth + highlightMarkerWidth / 2 - 0.5f;
            float highlightMarkerY = -(float)heatmapY / bitmapHeight - geneHit * (highlightMarkerHeight) - highlightMarkerHeight / 2 + 0.5f;

            highlightQuad.transform.localPosition = new Vector3(highlightMarkerX, highlightMarkerY, -0.001f);
            highlightQuad.transform.localScale = new Vector3(highlightMarkerWidth, highlightMarkerHeight, 1f);
            highlightQuad.SetActive(true);
            highlightInfoText.text = "";
            enlargedGeneText.gameObject.SetActive(true);
            enlargedGeneText.text = genes[geneHit];
            enlargedGeneText.transform.localPosition = new Vector3(enlargedGeneText.transform.localPosition.x,
                                                                highlightQuad.transform.localPosition.y + 0.077f, 0);
            return geneHit;
        }

        /// <summary>
        /// Highlights a gene in the genelist if it is there.
        /// For example when colouring from keyboard it draws attention to the gene in the list.
        /// </summary>
        /// <param name="geneName">The name of the gene.</param>
        public void HighLightGene(string geneName)
        {
            int geneHit = Array.FindIndex(genes, s => s.Equals(geneName, StringComparison.InvariantCultureIgnoreCase));
            if (geneHit != -1)
            {
                float highlightMarkerWidth = (float)geneListWidth / bitmapWidth;
                float highlightMarkerHeight = ((float)heatmapHeight / bitmapHeight) / genes.Length;
                float highlightMarkerX = (float)geneListX / bitmapWidth + highlightMarkerWidth / 2 - 0.5f;
                float highlightMarkerY = -(float)heatmapY / bitmapHeight - geneHit * (highlightMarkerHeight) - highlightMarkerHeight / 2 + 0.5f;
                //lineRenderer.SetColors(Color.red, Color.yellow);

                highlightGeneQuad.transform.localPosition = new Vector3(highlightMarkerX, highlightMarkerY, 0);
                highlightGeneQuad.transform.localScale = new Vector3(highlightMarkerWidth, highlightMarkerHeight, 1f);
                highlightGeneQuad.SetActive(true);
                highlightInfoText.text = "";
                highlightGeneText.text = genes[geneHit];
                highlightGeneText.transform.localPosition = new Vector3(highlightGeneText.transform.localPosition.x,
                                                    highlightGeneQuad.transform.localPosition.y + 0.09f, 0);
                highlight = true;
            }
            else
            {
                highlightGeneText.text = geneName + " not in the heatmap list";
            }

        }

        /// <summary>
        /// Handles the highlighting when the user is holding the trigger button to select multiple groups and genes. <paramref name="hitx"/> and <paramref name="hity"/> are determined on this frame,
        /// <paramref name="selectionStartX"/> and <paramref name="selectionStartY"/> were determined when the user first pressed the trigger.
        /// </summary>
        /// <param name="hitx">The last x coordinate that the raycast hit.</param>
        /// <param name="hity">The last y coordinate that the raycast hit.</param>
        /// <param name="selectionStartX">The first x coordinate that the raycast hit.</param>
        /// <param name="selectionStartY">The first y coordinate that the raycast hit.</param>
        public void HandleBoxSelection(int hitx, int hity, int selectionStartX, int selectionStartY)
        {
            // since the groupings have irregular widths we need to iterate over the list of widths
            float boxX = heatmapX;
            float boxWidth = 0;
            for (int i = 0; i < groupWidths.Count; ++i)
            {
                if (boxX + groupWidths[i].Item2 > hitx || boxX + groupWidths[i].Item2 > selectionStartX)
                {
                    do
                    {
                        boxWidth += groupWidths[i].Item2;
                        i++;
                    } while (boxX + boxWidth < hitx || boxX + boxWidth < selectionStartX);
                    break;
                }
                boxX += groupWidths[i].Item2;
            }

            float highlightMarkerWidth = boxWidth / bitmapWidth;
            float highlightMarkerX = boxX / bitmapWidth + highlightMarkerWidth / 2 - 0.5f;

            // the genes all have the same height so no need for loops here
            int geneHit1 = (int)((float)((bitmapHeight - hity) - heatmapY) / heatmapHeight * genes.Length);
            int geneHit2 = (int)((float)((bitmapHeight - selectionStartY) - heatmapY) / heatmapHeight * genes.Length);
            int smallerGeneHit = geneHit1 < geneHit2 ? geneHit1 : geneHit2;
            float highlightMarkerHeight = ((float)heatmapHeight / bitmapHeight) / genes.Length * (Math.Abs(geneHit1 - geneHit2) + 1);
            float highlightMarkerY = -((float)heatmapY + smallerGeneHit * ((float)heatmapHeight / genes.Length)) / bitmapHeight - highlightMarkerHeight / 2 + 0.5f;

            highlightQuad.transform.localPosition = new Vector3(highlightMarkerX, highlightMarkerY, -0.001f);
            highlightQuad.transform.localScale = new Vector3(highlightMarkerWidth, highlightMarkerHeight, 1f);
            highlightQuad.SetActive(true);
            highlightInfoText.text = "";

        }

        /// <summary>
        /// Checks if two coordinates are inside a rectangle.
        /// </summary>
        /// <param name="x">The x coordinate</param>
        /// <param name="y">The y coordinate</param>
        /// <param name="rectX">The rectangle's x coordinate</param>
        /// <param name="rectY">The rectangle's y coordinate</param>
        /// <param name="rectWidth">The rectangle's width</param>
        /// <param name="rectHeight">The rectangle's height</param>
        /// <returns></returns>
        private bool CoordinatesInsideRect(int x, int y, int rectX, int rectY, int rectWidth, int rectHeight)
        {
            return x >= rectX && y >= rectY && x < rectX + rectWidth && y < rectY + rectHeight;
        }

        /// <summary>
        /// Confirms the cells inside the rectangle drawn by the user. <paramref name="hitx"/> and <paramref name="hity"/> are determined on this frame,
        /// <paramref name="selectionStartX"/> and <paramref name="selectionStartY"/> were determined when the user first pressed the trigger.
        /// </summary>
        /// <param name="hitx">The last x coordinate that the raycast hit.</param>
        /// <param name="hity">The last y coordinate that the raycast hit.</param>
        /// <param name="selectionStartX">The first x coordinate that the raycast hit when the user first pressed the trigger.</param>
        /// <param name="selectionStartY">The first y coordinate that the raycast hit when the user first pressed the trigger.</param>
        public void ConfirmSelection(int hitx, int hity, int selectionStartX, int selectionStartY)
        {
            selecting = false;
            // since the groupings have irregular widths we need to iterate over the list of widths
            selectedBoxX = heatmapX;
            selectedBoxWidth = 0;

            selectedGroupLeft = 0;
            // the do while loop below increments selectedGroupRight one time too many, so start at -1
            selectedGroupRight = -1;
            selectedGeneBottom = 0;
            selectedGeneTop = 0;

            for (int i = 0; i < groupWidths.Count; ++i)
            {
                if (selectedBoxX + groupWidths[i].Item2 > hitx || selectedBoxX + groupWidths[i].Item2 > selectionStartX)
                {
                    do
                    {
                        selectedGroupRight++;
                        selectedBoxWidth += groupWidths[i].Item2;
                        i++;
                    } while (selectedBoxX + selectedBoxWidth < hitx || selectedBoxX + selectedBoxWidth < selectionStartX);
                    break;
                }
                selectedBoxX += groupWidths[i].Item2;
                selectedGroupLeft++;
                selectedGroupRight++;
            }

            float highlightMarkerWidth = selectedBoxWidth / bitmapWidth;
            float highlightMarkerX = selectedBoxX / bitmapWidth + highlightMarkerWidth / 2 - 0.5f;

            // the genes all have the same height so no need for loops here
            int geneHit1 = (int)((float)((bitmapHeight - hity) - heatmapY) / heatmapHeight * genes.Length);
            int geneHit2 = (int)((float)((bitmapHeight - selectionStartY) - heatmapY) / heatmapHeight * genes.Length);
            if (geneHit1 < geneHit2)
            {
                selectedGeneTop = geneHit1;
                selectedGeneBottom = geneHit2;
            }
            else
            {
                selectedGeneTop = geneHit2;
                selectedGeneBottom = geneHit1;
            }
            // have to add 1 at the end here so it includes the bottom row as well
            selectedBoxHeight = ((float)heatmapHeight) / genes.Length * (Math.Abs(geneHit1 - geneHit2) + 1);
            float highlightMarkerHeight = selectedBoxHeight / bitmapHeight;
            selectedBoxY = (float)heatmapY + selectedGeneTop * ((float)heatmapHeight / genes.Length);
            float highlightMarkerY = -(selectedBoxY) / bitmapHeight - highlightMarkerHeight / 2 + 0.5f;

            confirmQuad.transform.localPosition = new Vector3(highlightMarkerX, highlightMarkerY, -0.001f);
            confirmQuad.transform.localScale = new Vector3(highlightMarkerWidth, highlightMarkerHeight, 1f);
            confirmQuad.SetActive(true);

        }

        /// <summary>
        /// Moves the <see cref="movingQuadX"/> and <see cref="movingQuadY"/> when choosing where to move a selection
        /// </summary>
        /// <param name="hitx">The x coordinate where the raycast hit the heatmap</param>
        /// <param name="hity">The y coordinate where the raycast hit the heatmap</param>
        public void HandleMovingSelection(int hitx, int hity)
        {
            if (hitx < selectedBoxX || hitx > selectedBoxX + selectedBoxWidth)
            {
                float groupX, groupWidth;
                int group;
                FindGroupInfo(hitx, out groupX, out groupWidth, out group);
                if (hitx > groupX + groupWidth / 2f)
                {
                    groupX += groupWidth;
                }
                float highlightMarkerX = groupX / bitmapWidth + heatmapWidth / (2 * bitmapWidth) - 0.5f;
                movingQuadY.transform.localPosition = new Vector3(highlightMarkerX, 0f, -0.001f);
                movingQuadY.SetActive(true);
            }
            else
            {
                movingQuadY.SetActive(false);
            }
            if (bitmapHeight - hity < selectedBoxY || bitmapHeight - hity > selectedBoxY + selectedBoxHeight)
            {

                int geneHit = (int)(((bitmapHeight - hity + ((float)heatmapHeight / genes.Length) / 2) - heatmapY) / heatmapHeight * genes.Length);
                float highlightMarkerY = -((float)heatmapY + geneHit * ((float)heatmapHeight / genes.Length)) / bitmapHeight + 0.5f;
                movingQuadX.transform.localPosition = new Vector3(0f, highlightMarkerY, -0.001f);
                movingQuadX.SetActive(true);
            }
            else
            {
                movingQuadX.SetActive(false);
            }
        }

        /// <summary>
        /// Moves a part of the heatmap to another part. This can mean moving both rows and coloumns. Entire rows and coloumns and always moved and never split.
        /// </summary>
        /// <param name="hitx">The x coordinate where the selection should be moved to.</param>
        /// <param name="hity">The y coordinate where the selection should be moved to.</param>
        /// <param name="selectedGroupLeft">The lower index of the groups that should be moved.</param>
        /// <param name="selectedGroupRight">The higher index of the groups that should be moved.</param>
        /// <param name="selectedGeneTop">The lower index of the genes that should be moved.</param>
        /// <param name="selectedGeneBottom">The higher index of the genes that should be moved.</param>
        public void MoveSelection(int hitx, int hity, int selectedGroupLeft, int selectedGroupRight, int selectedGeneTop, int selectedGeneBottom)
        {
            movingSelection = false;
            bool recalculate = false;
            if (hitx < selectedBoxX || hitx > selectedBoxX + selectedBoxWidth)
            {
                int nbrOfGroups = selectedGroupRight - selectedGroupLeft + 1;
                int groupIndexToMoveTo = 0;
                float groupX = heatmapX;
                while (groupX + groupWidths[groupIndexToMoveTo].Item2 < hitx)
                {
                    groupX += groupWidths[groupIndexToMoveTo].Item2;
                    groupIndexToMoveTo++;
                }
                if (hitx > groupX + groupWidths[groupIndexToMoveTo].Item2 / 2f)
                {
                    groupIndexToMoveTo++;
                }

                List<Tuple<int, float, int>> groupWidthsToMove = new List<Tuple<int, float, int>>(nbrOfGroups);
                // add the groups we are moving to a temporary list
                groupWidthsToMove.AddRange(groupWidths.GetRange(selectedGroupLeft, nbrOfGroups));
                // we have to do this for both the groups and the cells
                // figure out the index that the first group is on in the cells list
                int cellsStartIndex = 0;
                foreach (Tuple<int, float, int> t in groupWidths.GetRange(0, selectedGroupLeft))
                {
                    cellsStartIndex += t.Item3;
                }
                int cellsStartIndexToMoveTo = 0;
                foreach (Tuple<int, float, int> t in groupWidths.GetRange(0, groupIndexToMoveTo))
                {
                    cellsStartIndexToMoveTo += t.Item3;
                }
                // figure out how many cells the groups cover in total
                int totalNbrOfCells = 0;
                foreach (Tuple<int, float, int> t in groupWidthsToMove)
                {
                    totalNbrOfCells += t.Item3;
                }
                // the correct index to move the groups to will have changed if the groups are moved to higher indices 
                if (groupIndexToMoveTo > selectedGroupRight)
                {
                    groupIndexToMoveTo -= nbrOfGroups;
                    cellsStartIndexToMoveTo -= totalNbrOfCells;
                }
                groupWidths.RemoveRange(selectedGroupLeft, nbrOfGroups);
                groupWidths.InsertRange(groupIndexToMoveTo, groupWidthsToMove);

                // here we need swap some stuff around in the cells array
                // figure out the index of the other part of the array that we need to move
                int otherPartStartIndex = cellsStartIndex < cellsStartIndexToMoveTo ? cellsStartIndex + totalNbrOfCells : cellsStartIndexToMoveTo;
                // figure out how many cells are inbetween the indeces. this is the same number of cells that the other part contains
                int numberOfcellsInOtherPart = Math.Abs(cellsStartIndex - cellsStartIndexToMoveTo);
                // figure out the index that the other part is moving to
                int otherPartIndexToMoveTo = cellsStartIndex < cellsStartIndexToMoveTo ? cellsStartIndex : cellsStartIndexToMoveTo + totalNbrOfCells;
                // temporary array with the cells we should move
                string[] cellsToMove = new string[totalNbrOfCells];
                // move the cells into the temporary array
                Array.Copy(cells, cellsStartIndex, cellsToMove, 0, totalNbrOfCells);
                // move the part we are swapping with to its new location
                Array.Copy(cells, otherPartStartIndex, cells, otherPartIndexToMoveTo, numberOfcellsInOtherPart);
                // move the cells from the temporary array to their new location
                Array.Copy(cellsToMove, 0, cells, cellsStartIndexToMoveTo, totalNbrOfCells);
                recalculate = true;
            }

            if (bitmapHeight - hity < selectedBoxY || bitmapHeight - hity > selectedBoxY + selectedBoxHeight)
            {
                int nbrOfGenes = selectedGeneBottom - selectedGeneTop + 1;
                int geneIndex = (int)(((bitmapHeight - hity + ((float)heatmapHeight / genes.Length) / 2) - heatmapY) / heatmapHeight * genes.Length);
                // Take the list of genes orignal genes
                List<string> original = new List<string>(genes);
                // make a temporary list with enough space for what should be moved
                List<string> temp = new List<string>(nbrOfGenes);
                // add what should be moved to the temporary list
                temp.AddRange(original.GetRange(selectedGeneTop, nbrOfGenes));
                // remove what should be moved from the original list
                original.RemoveRange(selectedGeneTop, nbrOfGenes);
                // recalculate the index if needed. Since we removed stuff from the original list the indeces might have shifted
                if (geneIndex > selectedGeneTop)
                {
                    geneIndex -= nbrOfGenes;
                }
                // insert what should be moved back into the original
                original.InsertRange(geneIndex, temp);
                genes = original.ToArray();
                recalculate = true;
            }
            if (recalculate)
            {
                // rebuild the heatmap texture
                BuildTexture(groupWidths);
            }
            ResetSelection();
        }

        /// <summary>
        /// Creates a new heatmap based on what was selected on this heatmap.
        /// </summary>
        public void CreateNewHeatmapFromSelection()
        {
            if (selectedBoxWidth == 0 || selectedBoxHeight == 0)
                return;

            // create a copy of this
            GameObject newHeatmap = Instantiate(gameObject);
            Heatmap heatmap = newHeatmap.GetComponent<Heatmap>();
            heatmap.transform.parent = referenceManager.heatmapGenerator.transform;
            heatmapGenerator.AddHeatmapToList(heatmap);
            heatmap.name = name + "_" + heatmapsCreated;
            heatmapsCreated++;
            heatmap.transform.Translate(0.1f, 0.1f, 0.1f, Space.Self);
            heatmap.groupingColors = groupingColors;
            // find out which indices the cells start and end at
            int cellsIndexStart = 0;
            for (int i = 0; i < selectedGroupLeft; ++i)
            {
                cellsIndexStart += groupWidths[i].Item3;
            }

            int numberOfCells = 0;
            for (int i = selectedGroupLeft; i <= selectedGroupRight; ++i)
            {
                numberOfCells += groupWidths[i].Item3;
            }

            string[] newCells = new string[numberOfCells];
            List<Graph.GraphPoint> newGps = new List<Graph.GraphPoint>();
            for (int i = 0, j = cellsIndexStart; i < numberOfCells; ++i, ++j)
            {
                newCells[i] = cells[j];
                newGps.Add(cellManager.GetCell(cells[j]).GraphPoints[0]);
            }

            string[] newGenes = new string[selectedGeneBottom - selectedGeneTop + 1];
            for (int i = selectedGeneTop, j = 0; i <= selectedGeneBottom; ++i, ++j)
            {
                newGenes[j] = genes[i];
            }

            // rebuild the groupwidth list with the new widths.
            List<Tuple<int, float, int>> newGroupWidths = new List<Tuple<int, float, int>>();
            float newXCoordInc = (float)heatmapWidth / newCells.Length;
            for (int i = selectedGroupLeft; i <= selectedGroupRight; ++i)
            {
                Tuple<int, float, int> old = groupWidths[i];
                newGroupWidths.Add(new Tuple<int, float, int>(old.Item1, old.Item3 * newXCoordInc, old.Item3));
            }
            // need to dump selection to txt file for GO analysis script. But file creation counter should not increment
            // in case networks should be created on the selection that created the original heatmap.
            referenceManager.selectionManager.DumpSelectionToTextFile(newGps);
            referenceManager.selectionManager.fileCreationCtr--;
            heatmap.Init();
            try
            {
                heatmap.BuildTexture(newCells, newGenes, newGroupWidths);
                DumpGenesToTextFile(newGenes, heatmap.name);
            }
            catch (Exception e)
            {
                CellexalLog.Log("Could not create heatmap. " + e.StackTrace);
            }
            heatmapGenerator.selectionNr += 1;
            heatmap.selectionNr = heatmapGenerator.selectionNr;
        }


        /// <summary>
        /// Dumps the genes into a text file. 
        /// </summary>
        private void DumpGenesToTextFile(string[] genes, string name)
        {
            string filePath = (CellexalUser.UserSpecificFolder + "\\Heatmap\\" + name + ".txt").FixFilePath();
            using (StreamWriter file = new StreamWriter(filePath))
            {
                foreach (string gene in genes)
                {
                    file.Write(gene);
                    file.WriteLine();
                }
            }
        }

        /// <summary>
        /// Resets the selection on the heatmap.
        /// </summary>
        private void ResetSelection()
        {
            confirmQuad.SetActive(false);
            movingQuadX.SetActive(false);
            movingQuadY.SetActive(false);

            selectedBoxX = 0;
            selectedBoxY = 0;
            selectedBoxHeight = 0;
            selectedBoxWidth = 0;

            selectedGeneBottom = 0;
            selectedGeneTop = 0;
            selectedGroupLeft = 0;
            selectedGroupRight = 0;
        }

        /// <summary>
        /// Updates this heatmap's image.
        /// </summary>
        [Obsolete("Use BuildTexture")]
        public void UpdateImage(string filepath)
        {
            byte[] fileData = File.ReadAllBytes(filepath);
            Texture2D tex = new Texture2D(2, 2);
            tex.LoadImage(fileData);
            GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
        }

        /// <summary>
        /// Saves the image used to create this heatmap to another directory.
        /// The saved image will have the a name based on when the image is saved.
        /// </summary>
        public void SaveImage()
        {
            string heatmapImageDirectory = CellexalUser.UserSpecificFolder;
            if (!Directory.Exists(heatmapImageDirectory))
            {
                Directory.CreateDirectory(heatmapImageDirectory);
                CellexalLog.Log("Created directory " + heatmapImageDirectory);
            }

            heatmapImageDirectory += "\\Heatmap";
            if (!Directory.Exists(heatmapImageDirectory))
            {
                Directory.CreateDirectory(heatmapImageDirectory);
                CellexalLog.Log("Created directory " + heatmapImageDirectory);
            }

            string heatmapImageFilePath = heatmapImageDirectory + "\\" + name + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png";
            while (File.Exists(heatmapImageFilePath))
            {
                // append "_d" until the filenames no longer collide.
                // microsoft is removing the 260 character filename limit so this shouldn't run into too many problems
                // unless you press this button way too many times the same second
                heatmapImageFilePath += "_d";
            }
            bitmap.Save(heatmapImageFilePath);
            StartCoroutine(LogHeatmap(heatmapImageFilePath));
        }

        /// <summary>
        /// Calls R logging function to save heatmap for session report.
        /// </summary>
        IEnumerator LogHeatmap(string heatmapImageFilePath)
        {
            removable = true;
            //CellexalEvents.ScriptRunning.Invoke();
            saveImageButton.SetButtonActivated(false);
            statusText.text = "Saving Heatmap...";
            string genesFilePath = (CellexalUser.UserSpecificFolder + "\\Heatmap\\" + name + ".txt").UnFixFilePath();
            string groupingsFilepath = (CellexalUser.UserSpecificFolder + "\\selection" + selectionNr + ".txt").UnFixFilePath();
            string rScriptFilePath = (Application.streamingAssetsPath + @"\R\logHeatmap.R").FixFilePath();
            string args = CellexalUser.UserSpecificFolder.UnFixFilePath() + " " + genesFilePath + " " + heatmapImageFilePath.UnFixFilePath() + " " + groupingsFilepath;

            while (referenceManager.selectionManager.RObjectUpdating || File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.input.R"))
            {
                yield return null;
            }

            CellexalLog.Log("Running R script " + rScriptFilePath + " with the arguments \"" + args + "\"");
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            Thread t = new Thread(() => RScriptRunner.RunRScript(rScriptFilePath, args));
            t.Start();

            while (t.IsAlive || File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.input.R"))
            {
                yield return null;
            }
            stopwatch.Stop();
            CellexalLog.Log("R log script finished in " + stopwatch.Elapsed.ToString());
            saveImageButton.FinishedButton();
            statusText.text = "";
            //CellexalEvents.ScriptFinished.Invoke();
            removable = false;
            //saveImageButton.SetButtonActivated(true);
        }

        /// <summary>
        /// Does a GO analysis of the genes on the heatmap. The Rscript does this and needs the genelist to do it.
        /// </summary>
        public void GOanalysis()
        {
            goAnalysisButton.SetButtonActivated(false);
            string goAnalysisDirectory = CellexalUser.UserSpecificFolder;
            if (!Directory.Exists(goAnalysisDirectory))
            {
                Directory.CreateDirectory(goAnalysisDirectory);
                CellexalLog.Log("Created directory " + goAnalysisDirectory);
            }

            goAnalysisDirectory += "\\Heatmap";
            if (!Directory.Exists(goAnalysisDirectory))
            {
                Directory.CreateDirectory(goAnalysisDirectory);
                CellexalLog.Log("Created directory " + goAnalysisDirectory);
            }
            StartCoroutine(GOAnalysis(goAnalysisDirectory));
        }

        /// <summary>
        /// Calls the R function with the filepath to the genes to analyse (this is the same as the heatmap directory).
        /// </summary>
        /// <param name="goAnalysisDirectory"></param>
        /// <returns></returns>
        IEnumerator GOAnalysis(string goAnalysisDirectory)
        {
            statusText.text = "Doing GO Analysis...";
            removable = true;
            //CellexalEvents.ScriptRunning.Invoke();
            string genesFilePath = (CellexalUser.UserSpecificFolder + "\\Heatmap\\" + name + ".txt").UnFixFilePath();
            string rScriptFilePath = (Application.streamingAssetsPath + @"\R\GOanalysis.R").FixFilePath();
            string groupingsFilepath = (CellexalUser.UserSpecificFolder + "\\selection" + selectionNr + ".txt").UnFixFilePath();
            string args = CellexalUser.UserSpecificFolder.UnFixFilePath() + " " + genesFilePath + " " + groupingsFilepath;

            while (referenceManager.selectionManager.RObjectUpdating || File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.input.R"))
            {
                yield return null;
            }

            Debug.Log("Running R script " + rScriptFilePath + " with the arguments \"" + args + "\"");
            CellexalLog.Log("Running R script " + rScriptFilePath + " with the arguments \"" + args + "\"");
            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            Thread t = new Thread(() => RScriptRunner.RunRScript(rScriptFilePath, args));
            t.Start();
            while (t.IsAlive || File.Exists(CellexalUser.UserSpecificFolder + "\\mainServer.input.R"))
            {
                yield return null;
            }
            stopwatch.Stop();
            CellexalLog.Log("R log script finished in " + stopwatch.Elapsed.ToString());
            statusText.text = "";
            goAnalysisButton.FinishedButton();
            removable = false;
            //CellexalEvents.ScriptFinished.Invoke();
            //goAnalysisButton.SetButtonActivated(true);
        }


        /// <summary>
        /// Recolours all graphs with the colors that the cells had when this heatmap was created.
        /// Graph points that are not part of this heatmap are not recoloured and will keep their colour.
        /// </summary>
        public void ColorCells()
        {
            var selectionManager = referenceManager.selectionManager;

            for (int i = 0, cellIndex = 0; i < groupWidths.Count; ++i)
            {

                int group = groupWidths[i].Item1;
                UnityEngine.Color groupColor = groupingColors[group];
                for (int j = 0; j < groupWidths[i].Item3; ++j, ++cellIndex)
                {
                    var graphPoint = cellManager.GetCell(cells[cellIndex]).GraphPoints[0];

                    selectionManager.AddGraphpointToSelection(graphPoint, group, false);
                }
            }
        }

        /// <summary>
        /// Sets some variables. Should be called after a heatmap is instantiated.
        /// </summary>
        public void SetVars(Dictionary<Cell, int> colors)
        {
            // containedCells = new Dictionary<Cell, Color>();
            //containedCells = colors;
            infoText.text = "Total number of cells: " + colors.Count;
            // infoText.text += "\nNumber of colours: " + numberOfColours;
        }
    }
}