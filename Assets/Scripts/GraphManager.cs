using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents a manager that holds all graphs.
/// </summary>
public class GraphManager : MonoBehaviour
{

    public CellManager cellManager;
    public Graph graphPrefab;
    public AudioSource goodSound;
    public AudioSource badSound;
    public SelectionToolHandler selectionToolHandler;
    private List<Graph> graphs;
    private Vector3[] startPositions =  {   new Vector3(-.7f, .5f, .1f),
                                            new Vector3(-.35f, .5f, -.7f),
                                            new Vector3(0f, .5f, .1f),
                                            new Vector3(.35f, .5f, -.7f)
                                        };

    void Awake()
    {
        graphs = new List<Graph>();
    }

    public void SetGraphStartPosition()
    {
        // these values are hard coded for your convenience

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
        Graph newGraph = Instantiate(graphPrefab, startPositions[(graphs.Count) % 4], Quaternion.identity);
        graphs.Add(newGraph);
        newGraph.transform.parent = transform;
        return newGraph;
    }

    public void DeleteGraphs()
    {
        cellManager.DeleteCells();
        foreach (Graph g in graphs)
        {
            if (g != null)
            {
                Destroy(g.gameObject);
            }
        }
        graphs.Clear();
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

    public void SetMinMaxCoords(Graph graph, Vector3 min, Vector3 max)
    {
        graph.SetMinMaxCoords(min, max);
    }

    public void ColorAllGraphsByGene(string geneName)
    {
        cellManager.ColorGraphsByGene(geneName);
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
    /// <param name="graphName"> The graph's name </param>
    /// <returns> A reference to the graph, or null if no graph was found </returns>
    public Graph FindGraph(string graphName)
    {
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
