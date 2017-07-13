using System;
using System.Collections.Generic;
using UnityEngine;

public class GraphManager : MonoBehaviour
{

    public CellManager cellManager;
    public Graph graphPrefab;
    public AudioSource goodSound;
    public AudioSource badSound;
    public SelectionToolHandler selectionToolHandler;
    private List<Graph> graphs;
    //private int activeGraph = 0;

    void Awake()
    {
        graphs = new List<Graph>();
    }

    //public void SetActiveGraph(int i)
    //{
    //    activeGraph = i;
    //}

    public void SetGraphStartPosition()
    {
        // these values are hard coded for your convenience
        graphs[0].transform.position = new Vector3(0.686f, .5f, -0.157f);
        graphs[1].transform.position = new Vector3(-.456f, .5f, -0.119f);
    }

    public Graph CreateGraph()
    {
        Graph newGraph = Instantiate(graphPrefab);
        graphs.Add(newGraph);
        // more hardcoded values
        //if (i == 0)
        //{
        //    graphs[0] = Instantiate(graphPrefab, new Vector3(0.686f, .5f, -0.157f), Quaternion.identity);
        //}
        //else if (i == 1)
        //{
        //    graphs[1] = Instantiate(graphPrefab, new Vector3(-.456f, .5f, -0.119f), Quaternion.identity);
        //}
        newGraph.transform.parent = this.transform;
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
    }

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

    public void ResetGraph()
    {
        selectionToolHandler.CancelSelection();
        foreach (Graph g in graphs)
        {

            g.ResetGraph();
        }
        SetGraphStartPosition();
    }

    public void CreateConvexHull(Graph graph)
    {
        graph.CreateConvexHull();
    }

    public void HideDDRGraph()
    {
        graphs[0].gameObject.SetActive(!graphs[0].gameObject.activeSelf);
    }

    public void HideTSNEGraph()
    {
        graphs[1].gameObject.SetActive(!graphs[1].gameObject.activeSelf); ;
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
