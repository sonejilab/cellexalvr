using System.Collections.Generic;
using UnityEngine;
using BayatGames.SaveGameFree.Examples;
using System;

/// <summary>
/// This class represents a manager that holds all graphs.
/// </summary>
public class GraphManager : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public Graph graphPrefab;
    public AudioSource goodSound;
    public AudioSource badSound;
    public SaveScene saveScene;
    public string directory;

    private CellManager cellManager;
    private SelectionToolHandler selectionToolHandler;
    private List<Graph> graphs;
    private List<NetworkHandler> networks = new List<NetworkHandler>();
    private Vector3[] startPositions =  {   new Vector3(-.2f, .5f, .3f),
                                            new Vector3(.3f, .5f, -.5f),
                                            new Vector3(0f, .5f, .1f),
                                            new Vector3(.35f, .5f, -.7f)
                                        };

    void Awake()
    {
        graphs = new List<Graph>();
    }

    private void Start()
    {
        cellManager = referenceManager.cellManager;
        selectionToolHandler = referenceManager.selectionToolHandler;
    }

    /// <summary>
    /// Finds a graphpoint.
    /// </summary>
    /// <param name="graphName"> The name of the graph the graphpoint is in. </param>
    /// <param name="label"> The graphpoint's label. </param>
    /// <returns> A reference to the graphpoint, or null if it was not found. </returns>
    public GraphPoint FindGraphPoint(string graphName, string label)
    {
        foreach (Graph g in graphs)
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

    /// <summary>
    /// Recolors a graphpoint.
    /// </summary>
    /// <param name="graphname"> The name of the graph. </param>
    /// <param name="label"> The graphpoint's label. </param>
    /// <param name="color"> The new color. </param>
    public void RecolorGraphPoint(string graphname, string label, Color color)
    {
        FindGraphPoint(graphname, label).Color = color;
    }

    /// <summary>
    /// Colors all graphs based on the graphpoints in the current selection.
    /// </summary>
    public void RecolorAllGraphsAfterSelection()
    {
        var selection = selectionToolHandler.GetCurrentSelection();
        if (selection.Count == 0)
        {
            // if the user has pressed the confirm selection button, but started a new selection yet
            // the graphs should be colored based on the previous selection
            selection = selectionToolHandler.GetLastSelection();
        }
        foreach (Graph graph in graphs)
        {
            foreach (GraphPoint point in selection)
            {
                graph.points[point.Label].Color = point.Color;
            }
        }
        CellExAlLog.Log("Recolored  " + selection.Count + " points in  " + graphs.Count + " graphs after current selection");
    }

    public void SetGraphStartPosition()
    {
        for (int i = 0; i < graphs.Count; ++i)
        {
            graphs[i].transform.position = startPositions[i % 4];
        }
    }

    /// <summary>
    /// Creates a new graph
    /// </summary>
    /// <returns> A reference to the newly created graph </returns>
    public Graph CreateGraph()
    {
        Graph newGraph = Instantiate(graphPrefab, startPositions[graphs.Count % 4], Quaternion.identity);
        //Debug.Log(newGraph.transform.position + " - " + saveScene.target1.position);
        newGraph.transform.parent = transform;
        newGraph.UpdateStartPosition();
        graphs.Add(newGraph);
        return newGraph;
    }

    public void LoadPosition(Graph graph, int graphNr)
    {
        saveScene.SetGraph(graph, graphNr);
        saveScene.LoadPositions();
        if (graphNr == 1)
        {
            graph.transform.position = saveScene.target1.position;
            graph.transform.rotation = saveScene.target1.rotation;
        }
        else if (graphNr == 2)
        {
            graph.transform.position = saveScene.target2.position;
            graph.transform.rotation = saveScene.target2.rotation;
        }
    }
    public void LoadDirectory()
    {
        saveScene.LoadDirectory();
        directory = saveScene.targetDir;
        Debug.Log("GM DIR: " + directory);
    }

    /// <summary>
    /// Deletes all graphs and networks in the scene.
    /// </summary>
    public void DeleteGraphsAndNetworks()
    {
        CellExAlLog.Log("Deleting graphs and networks");
        cellManager.DeleteCells();
        foreach (Graph g in graphs)
        {
            if (g != null)
            {
                Destroy(g.gameObject);
            }
        }
        graphs.Clear();
        foreach (NetworkHandler network in networks)
        {
            foreach (NetworkCenter networkReplacement in network.Replacements)
            {
                Destroy(networkReplacement.replacing.gameObject);
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

    public void SetMinMaxCoords(Graph graph, Vector3 min, Vector3 max)
    {
        graph.SetMinMaxCoords(min, max);
    }

    /// <summary>
    /// Resets all graphpoints' in all graphs colors to white.
    /// </summary>
    public void ResetGraphsColor()
    {
        selectionToolHandler.CancelSelection();
        foreach (Graph g in graphs)
        {
            g.ResetGraphColors();
        }
    }

    /// <summary>
    /// Resets the position, scale and color of all Graphs.
    /// </summary>
    public void ResetGraphs()
    {
        selectionToolHandler.CancelSelection();
        foreach (Graph g in graphs)
        {
            g.ResetGraph();
        }
        SetGraphStartPosition();
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
    public Graph FindGraph(string graphName)
    {
        if (graphName == "" && graphs.Count > 0)
        {
            return graphs[0];
        }
        foreach (Graph g in graphs)
        {
            if (g.GraphName == graphName)
            {
                return g;
            }
        }
        // no graph found
        return null;
    }
}
