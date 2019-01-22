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
    public string directory;
    public Material defaultGraphPointMaterial;
    public Shader graphPointNormalShader;
    public Shader graphPointOutlineShader;
    public SelectionToolHandler selectionToolHandler;

    public List<CombinedGraph> Graphs;

    private CellManager cellManager;
    private List<NetworkHandler> networks = new List<NetworkHandler>();
    private Vector3[] startPositions =  {   new Vector3(-0.2f, 1.1f, -0.95f),
                                            new Vector3(-0.9f, 1.1f, -0.4f),
                                            new Vector3(-0.9f, 1.1f, 0.4f),
                                            new Vector3(-0.2f, 1.1f, 0.95f)
                                        };

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

    [ConsoleCommand("graphManager", "drawdebugcubes", "ddc")]
    public void DrawDebugGizmos(int i)
    {
        if (i == 1)
        {
            drawDebugCubes = true;
        }
        else if (i == 0)
        {
            drawDebugCubes = false;
        }
    }

    [ConsoleCommand("graphManager", "drawdebuglines", "ddl")]
    public void DrawDebugLines(int i)
    {
        if (i == 1)
        {
            drawDebugLines = true;
        }
        else if (i == 0)
        {
            drawDebugLines = false;
        }
    }

    [ConsoleCommand("graphManager", "drawselectiontooldebuglines", "dstdl")]
    public void DrawSelectionToolDebugLines(int i)
    {
        if (i == 1)
        {
            drawSelectionToolDebugLines = true;
        }
        else if (i == 0)
        {
            drawSelectionToolDebugLines = false;
        }
    }

    [ConsoleCommand("graphManager", "drawraycast", "drc")]
    public void DrawDebugRaycast(int i)
    {
        if (i == 1)
        {
            drawDebugRaycast = true;
        }
        else if (i == 0)
        {
            drawDebugRaycast = false;
        }
    }

    [ConsoleCommand("graphManager", "drawrejectionapprovecubes", "drac")]
    public void DrawDebugRejectionApproveCubes(int i)
    {
        if (i == 1)
        {
            drawDebugRejectionApprovedCubes = true;
        }
        else if (i == 0)
        {
            drawDebugRejectionApprovedCubes = false;
        }
    }

    [ConsoleCommand("graphManager", "party")]
    public void Party(int i)
    {
        if (i == 1)
        {
            foreach (CombinedGraph graph in Graphs)
            {
                graph.Party();
            }
        }
        else if (i == 0)
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


    /// <summary>
    /// Get a material that can be used for coloring graphpoints in colors that are not defined by the config file.
    /// </summary>
    /// <param name="color">The desired color.</param>
    /// <returns>An existing material with the color of <paramref name="color"/> if one exists, or a new material if none existed.</returns>
    //  public Material GetAdditionalGroupingMaterial(UnityEngine.Color color, bool outline)
    //  {
    //      if (outline)
    //      {
    //          for (int i = 0; i < AdditionalGroupingMaterialsOutline.Count; ++i)
    //          {
    //              if (AdditionalGroupingMaterialsOutline[i].color.Equals(color))
    //              {
    //                  return AdditionalGroupingMaterialsOutline[i];
    //              }
    //          }
    //          var newMaterial = new Material(GeneExpressionMaterials[0]);
    //          float outlineR = color.r + (1 - color.r) / 2;
    //          float outlineG = color.g + (1 - color.g) / 2;
    //          float outlineB = color.b + (1 - color.b) / 2;
    //
    //          newMaterial.shader = graphPointOutlineShader;
    //          newMaterial.color = color;
    //          newMaterial.SetColor("_OutlineColor", new UnityEngine.Color(outlineR, outlineG, outlineB));
    //          AdditionalGroupingMaterialsOutline.Add(newMaterial);
    //          return newMaterial;
    //      }
    //      else
    //      {
    //          for (int i = 0; i < AdditionalGroupingMaterials.Count; ++i)
    //          {
    //              if (AdditionalGroupingMaterials[i].color.Equals(color))
    //              {
    //                  return AdditionalGroupingMaterials[i];
    //              }
    //          }
    //          var newMaterial = new Material(GeneExpressionMaterials[0]);
    //          newMaterial.color = color;
    //          AdditionalGroupingMaterials.Add(newMaterial);
    //          return newMaterial;
    //      }
    //  }

    /// <summary>
    /// Create the materials needed for recoloring graphpoints.
    /// </summary>
    //  private void OnConfigLoaded()
    //  {
    //      // Generate the materials needed by the selection tool.
    //      UnityEngine.Color[] selectionToolColors = CellexalConfig.SelectionToolColors;
    //      int numSelectionColors = selectionToolColors.Length;
    //      GroupingMaterials = new Material[numSelectionColors];
    //      GroupingMaterialsOutline = new Material[numSelectionColors];
    //
    //      for (int i = 0; i < numSelectionColors; ++i)
    //      {
    //          // Non-outlined version
    //          UnityEngine.Color selectionToolColor = selectionToolColors[i];
    //          Material selectedMaterial = new Material(defaultGraphPointMaterial);
    //          selectedMaterial.color = selectionToolColor;
    //          selectedMaterial.shader = graphPointNormalShader;
    //          GroupingMaterials[i] = selectedMaterial;
    //          // make the outline a bit lighter
    //          float outlineR = selectionToolColor.r + (1 - selectionToolColor.r) / 2;
    //          float outlineG = selectionToolColor.g + (1 - selectionToolColor.g) / 2;
    //          float outlineB = selectionToolColor.b + (1 - selectionToolColor.b) / 2;
    //
    //          // Outlined version
    //          Material selectedMaterialOutline = new Material(defaultGraphPointMaterial);
    //          selectedMaterialOutline.shader = graphPointOutlineShader;
    //          selectedMaterialOutline.color = selectionToolColors[i];
    //          selectedMaterialOutline.SetColor("_OutlineColor", new UnityEngine.Color(outlineR, outlineG, outlineB));
    //          GroupingMaterialsOutline[i] = selectedMaterialOutline;
    //      }
    //
    //      // Generate the materials used when coloring by gene expressions
    //      int nColors = CellexalConfig.NumberOfExpressionColors;
    //      GeneExpressionMaterials = new Material[nColors];
    //      UnityEngine.Color low = CellexalConfig.LowExpressionColor;
    //      UnityEngine.Color mid = CellexalConfig.MidExpressionColor;
    //      UnityEngine.Color high = CellexalConfig.HighExpressionColor;
    //
    //      var colors = CellexalExtensions.Extensions.InterpolateColors(low, mid, nColors / 2);
    //
    //      for (int i = 0; i < nColors / 2; ++i)
    //      {
    //          GeneExpressionMaterials[i] = new Material(defaultGraphPointMaterial);
    //          GeneExpressionMaterials[i].color = colors[i];
    //      }
    //
    //      colors = CellexalExtensions.Extensions.InterpolateColors(mid, high, nColors - nColors / 2);
    //
    //      for (int i = nColors / 2, j = 0; i < nColors; ++i, ++j)
    //      {
    //          GeneExpressionMaterials[i] = new Material(defaultGraphPointMaterial);
    //          GeneExpressionMaterials[i].color = colors[j];
    //      }
    //
    //      // Generate materials used when coloring by attribute
    //      UnityEngine.Color[] attributeColors = CellexalConfig.AttributeColors;
    //      AttributeMaterials = new Material[attributeColors.Length];
    //      for (int i = 0; i < attributeColors.Length; ++i)
    //      {
    //          Material attributeMaterial = new Material(defaultGraphPointMaterial);
    //          attributeMaterial.color = attributeColors[i];
    //          AttributeMaterials[i] = attributeMaterial;
    //      }
    //  }


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

    public void SetGraphStartPosition()
    {
        for (int i = 0; i < Graphs.Count; ++i)
        {
            Graphs[i].transform.position = startPositions[i % 4];
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
