using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Linq;
using CellexalVR.General;
using CellexalVR.AnalysisLogic;
using CellexalVR.DesktopUI;
using CellexalVR.Spatial;
using CellexalVR.AnalysisLogic;
using SQLiter;
using System.IO;
using System.Diagnostics;

namespace CellexalVR.AnalysisObjects
{

    /// <summary>
    /// Represents a manager that holds all graphs.
    /// </summary>
    public class GraphManager : MonoBehaviour

    {
        public ReferenceManager referenceManager;
        public AudioSource goodSound;
        public List<string> directories;
        public SelectionManager selectionManager;

        public List<Graph> Graphs;
        public List<Graph> originalGraphs;
        public List<Graph> facsGraphs;
        public List<Graph> attributeSubGraphs;
        public List<SpatialGraph> spatialGraphs;
        public List<string> velocityFiles;

        private CellManager cellManager;
        private List<NetworkHandler> networks = new List<NetworkHandler>();
        private bool axesVisible;
        private ArrayList expressionValues;

        /// <summary>
        /// The different methods for coloring graphs by gene expression. The different options are:
        /// <list>
        ///   <item>
        ///     <term>Linear:</term>
        ///     <description>Each color represent a range of expression values. All ranges are the same size.</description>
        ///   </item>
        ///   <item>
        ///     <term>Ranked:</term>
        ///     <description>Each color contains the same number of cells.</description>
        ///   </item>
        /// </list>
        /// </summary>
        public enum GeneExpressionColoringMethods { EqualExpressionRanges, EqualCellNumbers };
        public GeneExpressionColoringMethods GeneExpressionColoringMethod = GeneExpressionColoringMethods.EqualExpressionRanges;

#if UNITY_EDITOR
        [Header("Debuging")]
        public bool drawDebugCubes = false;
        public bool drawDebugLines = false;
        public bool drawSelectionToolDebugLines = false;
        public bool drawDebugRaycast = false;
        public bool drawDebugRejectionApprovedCubes = false;
        public bool drawDebugGroups = false;
        public int drawDebugCubesOnLevel = -1;
#endif

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        void Awake()
        {
            Graphs = new List<Graph>();
        }

        private void Start()
        {
            cellManager = referenceManager.cellManager;
            selectionManager = referenceManager.selectionManager;
        }

        private void OnEnable()
        {
            //CellexalEvents.ConfigLoaded.AddListener(OnConfigLoaded);
        }

        private void OnDisable()
        {
            //CellexalEvents.ConfigLoaded.RemoveListener(OnConfigLoaded);
        }

#if UNITY_EDITOR
        #region DEBUG_FUNCTIONS

        [ConsoleCommand("graphManager", aliases: new string[] { "drawdebugcubes", "ddc" })]
        public void DrawDebugGizmos(bool b)
        {
            drawDebugCubes = b;
            CellexalEvents.CommandFinished.Invoke(true);
        }

        [ConsoleCommand("graphManager", aliases: new string[] { "drawdebuglines", "ddl" })]
        public void DrawDebugLines(bool b)
        {
            drawDebugLines = b;
            CellexalEvents.CommandFinished.Invoke(true);
        }

        [ConsoleCommand("graphManager", aliases: new string[] { "drawselectiontooldebuglines", "dstdl" })]
        public void DrawSelectionToolDebugLines(bool b)
        {
            drawSelectionToolDebugLines = b;
            CellexalEvents.CommandFinished.Invoke(true);
        }

        [ConsoleCommand("graphManager", aliases: new string[] { "drawraycast", "drc" })]
        public void DrawDebugRaycast(bool b)
        {
            drawDebugRaycast = b;
            CellexalEvents.CommandFinished.Invoke(true);
        }

        [ConsoleCommand("graphManager", aliases: new string[] { "drawrejectionapprovecubes", "drac" })]
        public void DrawDebugRejectionApproveCubes(bool b)
        {
            drawDebugRejectionApprovedCubes = b;
            CellexalEvents.CommandFinished.Invoke(true);
        }

        [ConsoleCommand("graphManager", aliases: new string[] { "drawdebugcubesonlevel", "ddcol" })]
        public void DrawDebugCubesOnLevel(int level)
        {
            drawDebugCubesOnLevel = level;
            CellexalEvents.CommandFinished.Invoke(true);
        }

        [ConsoleCommand("graphManager", aliases: "party")]
        public void Party(bool b)
        {
            if (b)
            {
                foreach (Graph graph in Graphs)
                {
                    graph.Party();
                }
            }
            else
            {
                foreach (Graph graph in Graphs)
                {
                    graph.ResetColors();
                }
            }
            CellexalEvents.CommandFinished.Invoke(true);
        }

        [ConsoleCommand("graphManager", aliases: new string[] { "drawdebuggroups", "ddg" })]
        public void DrawDebugGroups(bool b)
        {
            drawDebugGroups = b;
            CellexalEvents.CommandFinished.Invoke(true);
        }

        [ConsoleCommand("graphManager", aliases: new string[] { "tt" })]
        public void TestTexture()
        {
            var graph = Graphs[0];
            for (int i = 0; i < 256; ++i)
            {
                graph.texture.SetPixels32(i, 0, 1, 1, new Color32[] { new Color32((byte)i, 0, 0, 1) });
            }
            graph.texture.Apply();

            var data = graph.texture.GetRawTextureData();
            for (int i = 0; i < data.Length; i += 4)
            {
                print(i + ": " + data[i] + " " + data[i + 1] + " " + data[i + 2] + " " + data[i + 3]);
            }
        }

        #endregion
#endif
        /// <summary>
        /// Finds a graphpoint.
        /// </summary>
        /// <param name="graphName"> The name of the graph the graphpoint is in. </param>
        /// <param name="label"> The graphpoint's label. </param>
        /// <returns> A reference to the graphpoint, or null if it was not found. </returns>
        public Graph.GraphPoint FindGraphPoint(string graphName, string label)
        {
            foreach (Graph g in Graphs)
            {
                if (g.GraphName.Equals(graphName))
                {
                    if (g.points.ContainsKey(label))
                        return g.points[label];
                    else
                        return null;
                }
            }
            return null;
        }

        [ConsoleCommand("graphManager", aliases: new string[] { "checkfornullnodes", "cnn" })]
        public void CheckNullNodes()
        {
            foreach (var c in cellManager.GetCells())
            {
                foreach (var gp in c.GraphPoints)
                {
                    if (gp.node == null)
                        CellexalLog.Log(gp.Label + " " + gp.node);
                }
            }
        }


        [ConsoleCommand("graphManager", aliases: "cg")]
        public void RecolorGraphPoint(string label, int i)
        {
            foreach (var graph in Graphs)
            {
                var gp = graph.FindGraphPoint(label);
                graph.ColorGraphPointSelectionColor(gp, i, false);
            }
            CellexalEvents.CommandFinished.Invoke(true);
        }

        [ConsoleCommand("graphManager", aliases: "colorfeature")]
        public void ColorGraphsByFeature(string featureName)
        {
            StartCoroutine(ColorGraphsByFeatureCoroutine(featureName));
        }

        private IEnumerator ColorGraphsByFeatureCoroutine(string featureName)
        {
            string filePath = featureName + ".mtx";
            // string geneName = filePath.Replace(".txt", "");

            yield return StartCoroutine(ReadExpressions(filePath));


            ColorAllGraphsByGeneExpression(featureName, expressionValues);
            PythonInterpreter.WriteToOutput(
                $"Colored {expressionValues.Count} cells according to the expression of {featureName}");
        }

        private IEnumerator ReadExpressions(string filePath)
        {
            const float maximumDeltaTime = 0.05f; // 20 fps
            int maximumItemsPerFrame = CellexalConfig.Config.GraphLoadingCellsPerFrameStartCount;
            expressionValues = new ArrayList();
            GeneExpressionColoringMethods coloringMethod = GeneExpressionColoringMethods.EqualExpressionRanges;
            float LowestExpression = float.MaxValue;
            float HighestExpression = float.MinValue;
            Stopwatch stopwatch = new Stopwatch();
            if (coloringMethod == GraphManager.GeneExpressionColoringMethods.EqualExpressionRanges)
            {
                stopwatch.Start();
                using (StreamReader streamReader = new StreamReader(filePath))
                {
                    string line = streamReader.ReadLine();
                    while (line.Contains("%"))
                    {
                        // skip header lines of mtx file.
                        line = streamReader.ReadLine();
                    }

                    // string header = streamReader.ReadLine();
                    // string info = streamReader.ReadLine();
                    int i = 0;
                    while (!streamReader.EndOfStream)
                    {
                        int itemsThisFrame = 0;
                        for (int j = 0; j < maximumItemsPerFrame && !streamReader.EndOfStream; ++j)
                        {
                            string[] words = streamReader.ReadLine().Split(null);
                            string cellName = cellManager.cellNames[int.Parse(words[0]) - 1]; // if using R then indexing needs offset by 1.
                            float expr = float.Parse(words[2]);


                            if (expr > HighestExpression)
                            {
                                HighestExpression = expr;
                            }

                            if (expr < LowestExpression)
                            {
                                LowestExpression = expr;
                            }

                            itemsThisFrame++;
                            expressionValues.Add(new CellExpressionPair(cellName, expr, -1));
                        }

                        i += itemsThisFrame;

                        yield return null;
                        float lastFrame = Time.deltaTime;
                        if (lastFrame < maximumDeltaTime)
                        {
                            // we had some time over last frame
                            maximumItemsPerFrame += CellexalConfig.Config.GraphLoadingCellsPerFrameIncrement;
                        }
                        else if (lastFrame > maximumDeltaTime && maximumItemsPerFrame >
                            CellexalConfig.Config.GraphLoadingCellsPerFrameIncrement * 2)
                        {
                            // we took too much time last frame
                            maximumItemsPerFrame -= CellexalConfig.Config.GraphLoadingCellsPerFrameIncrement;
                        }
                    }

                    if (HighestExpression == LowestExpression)
                    {
                        HighestExpression += 1;
                    }

                    // increase highest expresion slightly so the actually highest expressed cell get in the correct group
                    HighestExpression *= 1.0001f;
                    float binSize = (HighestExpression - LowestExpression) /
                                    CellexalConfig.Config.GraphNumberOfExpressionColors;

                    foreach (CellExpressionPair pair in expressionValues)
                    {
                        pair.Color = (int)((pair.Expression - LowestExpression) / binSize);
                    }

                    stopwatch.Stop();
                    print($"Colored in {stopwatch.Elapsed} seconds, {i} items total.");
                }
            }
            else
            {

            }
        }


        /// <summary>
        /// Color all graphs with the expression of some gene.
        /// </summary>
        /// <param name="expressions">An arraylist with <see cref="SQLiter.CellExpressionPair"/>.</param>
        public void ColorAllGraphsByGeneExpression(string geneName, ArrayList expressions)
        {
            if (TextureHandler.instance.textureCoordDict.Count > 0)
            {
                TextureHandler.instance.ColorByExpression(expressions);
            }

            foreach (Graph graph in Graphs)
            {
                graph.ColorByGeneExpression(expressions);
            }

            // create the gene expression histogram
            int numberOfBins = CellexalConfig.Config.GraphNumberOfExpressionColors + 1;
            int[] cellsPerBin = new int[numberOfBins];
            float highestExpression = referenceManager.database.HighestExpression;
            foreach (SQLiter.CellExpressionPair expression in expressions)
            {
                cellsPerBin[expression.Color + 1]++;
            }
            cellsPerBin[0] = referenceManager.cellManager.GetNumberOfCells() - expressions.Count;
            referenceManager.legendManager.desiredLegend = LegendManager.Legend.GeneExpressionLegend;
            referenceManager.legendManager.geneExpressionHistogram.CreateHistogram(geneName, cellsPerBin, highestExpression.ToString(), GeneExpressionHistogram.YAxisMode.Linear);
            if (referenceManager.legendManager.currentLegend != referenceManager.legendManager.desiredLegend)
            {
                referenceManager.legendManager.ActivateLegend(referenceManager.legendManager.desiredLegend);
            }
        }

        /// <summary>
        /// Colors all graphs by a selection.
        /// </summary>
        /// <param name="selection">The selection to color by.</param>
        public void ColorAllGraphsBySelection(Selection selection)
        {
            foreach (Graph graph in Graphs)
            {
                graph.ColorBySelection(selection);
            }
        }

        /// <summary>
        /// Deletes all graphs and networks in the scene.
        /// </summary>
        public void DeleteGraphsAndNetworks()
        {
            CellexalLog.Log("Deleting graphs and networks");
            cellManager.DeleteCells();
            foreach (Graph g in Graphs)
            {
                if (g != null)
                {
                    Destroy(g.gameObject);
                }
            }
            Graphs.Clear();
            originalGraphs.Clear();
            facsGraphs.Clear();
            attributeSubGraphs.Clear();
            foreach (NetworkHandler network in networks)
            {
                foreach (NetworkCenter networkReplacement in network.Replacements)
                {
                    try
                    {
                        Destroy(networkReplacement.replacing.gameObject);
                    }
                    catch (Exception)
                    {

                    }
                }
                Destroy(network.gameObject);
            }
            networks.Clear();
        }

        public void AddNetwork(NetworkHandler handler)
        {
            networks.Add(handler);
        }

        public void RemoveNetwork(NetworkHandler handler)
        {
            networks.Remove(handler);
        }


        /// <summary>
        /// Toggles the transparency of all graph points on/off.
        /// </summary>
        /// <param name="toggle"></param>
        public void ToggleGraphPointTransparency(bool toggle)
        {
            foreach (Graph graph in Graphs)
            {
                graph.MakeAllPointsTransparent(toggle);
            }
        }

        /// <summary>
        /// Clears expression colours from graph but keeps current selection colours.
        /// </summary>
        public void ClearExpressionColours()
        {
            foreach (Graph g in Graphs)
            {
                g.ClearTopExprCircles();
                g.ResetColors(resetGroup: false);
            }
            selectionManager.RecolorSelectionPoints();
            CellexalEvents.GraphsResetKeepSelection.Invoke();
        }

        /// <summary>
        /// Resets all graphpoints' in all graphs colors to white.
        /// </summary>
        [ConsoleCommand("graphManager", aliases: new string[] { "resetcolor", "rc" })]
        public void ResetGraphsColor()
        {
            //selectionManager.CancelSelection();
            selectionManager.Clear();
            foreach (Graph g in Graphs)
            {
                g.ClearTopExprCircles();
                g.ResetColors();
            }
            if (TextureHandler.instance.textureCoordDict.Count > 0)
            {
                TextureHandler.instance.ResetTexture();
            }
            CellexalEvents.CommandFinished.Invoke(true);
            CellexalEvents.GraphsReset.Invoke();
        }

        /// <summary>
        /// Resets the position, scale and color of all Graphs.
        /// </summary>
        public void ResetGraphsPosition()
        {
            foreach (Graph g in Graphs)
            {
                if (g.GraphName.Contains("Slice"))
                {
                    continue;
                }
                g.ResetPosition();
                g.ResetSizeAndRotation();
            }
            foreach (SpatialGraph sg in spatialGraphs)
            {
                sg.ResetPosition();
                sg.ResetSizeAndRotation();
            }
            //SetGraphStartPosition();
        }

        /// <summary>
        /// Delete graph using the delete tool.
        /// </summary>
        /// <param name="name">Name of the gameobject</param>
        /// <param name="tag">Tag of the graph. Behaves differently depending on if it is subgraph or facsgraph. Original graphs cannot be removed.</param>
        public void DeleteGraph(string name, string tag)
        {
            Graph graph = FindGraph(name);
            if (graph != null)
            {
                graph.DeleteGraph(tag);
            }
        }

        /// <summary>
        /// Finds a graph.
        /// </summary>
        /// <param name="graphName"> The graph's name, or an empty string for any graph. </param>
        /// <returns> A reference to the graph, or null if no graph was found </returns>
        public Graph FindGraph(string graphName, bool caseSensitive = false)
        {
            if (graphName == "" && Graphs.Count > 0)
            {
                return Graphs[0];
            }

            if (!caseSensitive)
            {
                foreach (Graph g in Graphs)
                {
                    if (g.GraphName == graphName)
                    {
                        return g;
                    }
                }
            }
            else
            {
                graphName = graphName.ToLower();
                foreach (Graph g in Graphs)
                {
                    if (g.GraphName.ToLower() == graphName)
                    {
                        return g;
                    }
                }
            }
            // no graph found
            return null;
        }
        public SpatialGraph FindSpatialGraph(string graphName)
        {
            if (graphName == "" && spatialGraphs.Count > 0)
            {
                return spatialGraphs[0];
            }
            foreach (SpatialGraph g in spatialGraphs)
            {
                if (g.gameObject.name == graphName)
                {
                    return g;
                }
            }
            // no graph found
            return null;
        }

        /// <summary>
        /// Removes all lines between graphpoints.
        /// </summary>
        public void ClearLinesBetweenGraphs()
        {
            foreach (Graph g in originalGraphs)
            {
                for (int i = 0; i < g.ctcGraphs.Count; i++)
                {
                    GraphBetweenGraphs gbg = g.ctcGraphs[i].GetComponent<GraphBetweenGraphs>();
                    gbg.RemoveGraph();
                }
                g.ctcGraphs.Clear();
            }
            foreach (Graph g in attributeSubGraphs)
            {
                for (int i = 0; i < g.ctcGraphs.Count; i++)
                {
                    GraphBetweenGraphs gbg = g.ctcGraphs[i].GetComponent<GraphBetweenGraphs>();
                    gbg.RemoveGraph();
                }
                g.ctcGraphs.Clear();

            }
            foreach (Graph g in facsGraphs)
            {
                for (int i = 0; i < g.ctcGraphs.Count; i++)
                {
                    GraphBetweenGraphs gbg = g.ctcGraphs[i].GetComponent<GraphBetweenGraphs>();
                    gbg.RemoveGraph();
                }
                g.ctcGraphs.Clear();

            }
        }

        /// <summary>
        /// Set all graphs' info panels to visible or not visible.
        /// </summary>
        /// <param name="visible"> TRue for visible, false for invisible </param>
        public void ToggleInfoPanels()
        {
            foreach (Graph g in Graphs)
            {
                g.ToggleInfoText();
            }
        }

        /// <summary>
        /// Set all graphs' axes to visible or not visible.
        /// </summary>
        /// <param name="visible"> TRue for visible, false for invisible </param>
        public void ToggleAxes()
        {
            axesVisible = !axesVisible;
            foreach (Graph g in Graphs)
            {
                g.SetAxesVisible(axesVisible);
            }
        }

    }
}