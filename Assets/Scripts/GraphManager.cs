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
    private Graph[] graphs;
    private int activeGraph = 0;

    void Awake()
    {
        graphs = new Graph[2];
    }

    public void SetActiveGraph(int i)
    {
        activeGraph = i;
    }

    public void SetGraphStartPosition()
    {
        // these values are hard coded for your convenience
        graphs[0].transform.position = new Vector3(0.686f, .5f, -0.157f);
        graphs[1].transform.position = new Vector3(-.456f, .5f, -0.119f);
    }

    public void CreateGraph(int i)
    {
        // more hardcoded values
        if (i == 0)
        {
            graphs[0] = Instantiate(graphPrefab, new Vector3(0.686f, .5f, -0.157f), Quaternion.identity);
        }
        else if (i == 1)
        {
            graphs[1] = Instantiate(graphPrefab, new Vector3(-.456f, .5f, -0.119f), Quaternion.identity);
        }
        graphs[i].gameObject.SetActive(true);
        graphs[i].transform.parent = this.transform;
    }

    public void ColorAllGraphsByAttribute(string attribute)
    {

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

    public void AddCell(string label, float x, float y, float z)
    {
        graphs[activeGraph].AddGraphPoint(cellManager.AddCell(label), x, y, z);
    }

    public void SetMinMaxCoords(Vector3 min, Vector3 max)
    {
        graphs[activeGraph].SetMinMaxCoords(min, max);
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
    public void CreateConvexHull(int graph)
    {
        graphs[graph].CreateConvexHull(graph);
    }

    public void HideDDRGraph()
    {
        graphs[0].gameObject.SetActive(!graphs[0].gameObject.activeSelf);
    }

    public void HideTSNEGraph()
    {
        graphs[1].gameObject.SetActive(!graphs[1].gameObject.activeSelf); ;
    }
}
