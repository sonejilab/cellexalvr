using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;
using System.Drawing;

/// <summary>
/// Represents a manager that holds all graphs.
/// </summary>
public class GraphManager : MonoBehaviour

{
    public ReferenceManager referenceManager;
    public CombinedGraph combinedGraphPrefab;
    public AudioSource goodSound;
    public AudioSource badSound;
    public List<string> directories;
    public Material defaultGraphPointMaterial;
    public Shader graphPointNormalShader;
    public Shader graphPointOutlineShader;
    public SelectionToolHandler selectionToolHandler;

    public List<CombinedGraph> Graphs;

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

    // public Material[] GeneExpressionMaterials;
    // public Material[] GroupingMaterials;
    // public Material[] GroupingMaterialsOutline;
    // public Material[] AttributeMaterials;
    // if additional grouping materials need to be created because of previous groupings
    //public List<Material> AdditionalGroupingMaterials;
    //public List<Material> AdditionalGroupingMaterialsOutline;
    [Header("Debuging")]
    public bool drawDebugCubes = false;
    public bool drawDebugLines = false;
    public bool drawSelectionToolDebugLines = false;
    public bool drawDebugRaycast = false;
    public bool drawDebugRejectionApprovedCubes = false;
    public bool drawDebugGroups = false;

    void Awake()
    {
        Graphs = new List<CombinedGraph>();
    }

    private void Start()
    {
        cellManager = referenceManager.cellManager;
        selectionToolHandler = referenceManager.selectionToolHandler;
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

    [ConsoleCommand("graphManager", "party")]
    public void Party(bool b)
    {
        if (b)
        {
            foreach (CombinedGraph graph in Graphs)
            {
                graph.Party();
            }
        }
        else
        {
            foreach (CombinedGraph graph in Graphs)
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
    public CombinedGraph.CombinedGraphPoint FindGraphPoint(string graphName, string label)
    {
        foreach (CombinedGraph g in Graphs)
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
        foreach (CombinedGraph graph in Graphs)
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
        foreach (CombinedGraph g in Graphs)
        {
            if (g != null)
            {
                Destroy(g.gameObject);
            }
        }
        Graphs.Clear();
        foreach (NetworkHandler network in networks)
        {
            foreach (NetworkCenter networkReplacement in network.Replacements)
            {
                try
                {
                    Destroy(networkReplacement.replacing.gameObject);
                }
                catch (Exception e)
                {

                }
            }
            Destroy(network.gameObject);
        }
        networks.Clear();
    }

    /// <summary>
    /// Adds a cell to a graph.
    /// </summary>
    /// <param name="graph"> The graph the cell should belong to. </param>
    /// <param name="label"> The cell's name. </param>
    /// <param name="x"> The cell's x-coordinate. </param>
    /// <param name="y"> The cell's y-coordinate. </param>
    /// <param name="z"> The cell's z-coordinate. </param>
    public void AddCell(Graph graph, string label, float x, float y, float z)
    {
        graph.AddGraphPoint(cellManager.AddCell(label), x, y, z);
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
        selectionToolHandler.CancelSelection();
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
        selectionToolHandler.CancelSelection();
        foreach (CombinedGraph g in Graphs)
        {
            g.ResetColorsAndPosition();
            g.ResetSizeAndRotation();
        }
        //SetGraphStartPosition();
    }

    /// <summary>
    /// Creates a funny looking skeleton of a graph.
    /// </summary>
    public void CreateConvexHull(Graph graph)
    {
        graph.CreateConvexHull();
    }

    /// <summary>
    /// Finds a graph.
    /// </summary>
    /// <param name="graphName"> The graph's name, or an empty string for any graph. </param>
    /// <returns> A reference to the graph, or null if no graph was found </returns>
    public CombinedGraph FindGraph(string graphName)
    {
        if (graphName == "" && Graphs.Count > 0)
        {
            return Graphs[0];
        }
        foreach (CombinedGraph g in Graphs)
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
        foreach (CombinedGraph g in Graphs)
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
        foreach (CombinedGraph g in Graphs)
        {
            g.SetInfoTextVisible(visible);
        }
    }
}
