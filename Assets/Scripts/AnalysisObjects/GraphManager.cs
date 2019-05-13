using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using CellexalVR.General;
using CellexalVR.AnalysisLogic;
using CellexalVR.DesktopUI;

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
        public Shader graphPointNormalShader;
        public Shader graphPointOutlineShader;
        //public SelectionToolHandler selectionToolHandler;
        public SelectionManager selectionManager;

        public List<Graph> Graphs;
        public List<Graph> originalGraphs;

        private CellManager cellManager;
        private List<NetworkHandler> networks = new List<NetworkHandler>();

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
        public enum GeneExpressionColoringMethods { Linear, Ranked };
        public GeneExpressionColoringMethods GeneExpressionColoringMethod = GeneExpressionColoringMethods.Linear;

        [Header("Debuging")]
        public bool drawDebugCubes = false;
        public bool drawDebugLines = false;
        public bool drawSelectionToolDebugLines = false;
        public bool drawDebugRaycast = false;
        public bool drawDebugRejectionApprovedCubes = false;
        public bool drawDebugGroups = false;
        public int drawDebugCubesOnLevel = -1;

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

        #region DEBUG_FUNCTIONS

        [ConsoleCommand("graphManager", "drawdebugcubes", "ddc")]
        public void DrawDebugGizmos(bool b)
        {
            drawDebugCubes = b;
        }

        [ConsoleCommand("graphManager", "drawdebuglines", "ddl")]
        public void DrawDebugLines(bool b)
        {
            drawDebugLines = b;
        }

        [ConsoleCommand("graphManager", "drawselectiontooldebuglines", "dstdl")]
        public void DrawSelectionToolDebugLines(bool b)
        {
            drawSelectionToolDebugLines = b;
        }

        [ConsoleCommand("graphManager", "drawraycast", "drc")]
        public void DrawDebugRaycast(bool b)
        {
            drawDebugRaycast = b;
        }

        [ConsoleCommand("graphManager", "drawrejectionapprovecubes", "drac")]
        public void DrawDebugRejectionApproveCubes(bool b)
        {
            drawDebugRejectionApprovedCubes = b;
        }

        [ConsoleCommand("graphManager", "drawdebugcubesonlevel", "ddcol")]
        public void DrawDebugCubesOnLevel(int level)
        {
            drawDebugCubesOnLevel = level;
        }

        [ConsoleCommand("graphManager", "party")]
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
        }

        [ConsoleCommand("graphManager", "drawdebuggroups", "ddg")]
        public void DrawDebugGroups(bool b)
        {
            drawDebugGroups = b;
        }

        #endregion

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

        [ConsoleCommand("graphManager", "cg")]
        public void RecolorGraphPoint(string label, int i)
        {
            foreach (var graph in Graphs)
            {
                graph.RecolorGraphPointSelectionColor(graph.FindGraphPoint(label), i, false);
            }
        }

        /// <summary>
        /// Recolors a graphpoint.
        /// </summary>
        /// <param name="graphname"> The name of the graph. </param>
        /// <param name="label"> The graphpoint's label. </param>
        /// <param name="color"> The new color. </param>
        public void RecolorGraphPoint(string graphname, string label, UnityEngine.Color color)
        {
            //FindGraphPoint(graphname, label).Recolor(color, false);
        }

        public void ColorAllGraphsByGeneExpression(ArrayList expressions)
        {
            foreach (Graph graph in Graphs)
            {
                graph.ColorByGeneExpression(expressions);
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
        /// Resets all graphpoints' in all graphs colors to white.
        /// </summary>
        [ConsoleCommand("graphManager", "resetcolor", "rc")]
        public void ResetGraphsColor()
        {
            CellexalEvents.GraphsReset.Invoke();
            selectionManager.CancelSelection();
            foreach (var g in Graphs)
            {
                g.ResetColors();
            }
        }

        /// <summary>
        /// Resets the position, scale and color of all Graphs.
        /// </summary>
        public void ResetGraphs()
        {
            CellexalEvents.GraphsReset.Invoke();
            selectionManager.CancelSelection();
            foreach (Graph g in Graphs)
            {
                g.ResetColorsAndPosition();
                g.ResetSizeAndRotation();
            }
            //SetGraphStartPosition();
        }

        /// <summary>
        /// Finds a graph.
        /// </summary>
        /// <param name="graphName"> The graph's name, or an empty string for any graph. </param>
        /// <returns> A reference to the graph, or null if no graph was found </returns>
        public Graph FindGraph(string graphName)
        {
            if (graphName == "" && Graphs.Count > 0)
            {
                return Graphs[0];
            }
            foreach (Graph g in Graphs)
            {
                if (g.GraphName == graphName)
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
            foreach (Graph g in Graphs)
            {
                g.Lines.Clear();
            }
        }

        /// <summary>
        /// Set all graphs' info panels to visible or not visible.
        /// </summary>
        /// <param name="visible"> TRue for visible, false for invisible </param>
        public void SetInfoPanelsVisible(bool visible)
        {
            foreach (Graph g in Graphs)
            {
                g.SetInfoTextVisible(visible);
            }
        }

        /// <summary>
        /// Set all graphs' axes to visible or not visible.
        /// </summary>
        /// <param name="visible"> TRue for visible, false for invisible </param>
        public void SetAxesVisible(bool visible)
        {
            foreach (Graph g in Graphs)
            {
                g.SetAxesVisible(visible);
            }
        }

    }
}