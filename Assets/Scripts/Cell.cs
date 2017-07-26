using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents one cell. A cell may be present in multiple graphs.
/// </summary>
public class Cell
{

    private string labelString;
    private Dictionary<string, string> attributes;
    private Dictionary<string, int> facs;
    //private Dictionary<string, int> geneExpressions;
    private List<GraphPoint> graphPoints;
    private List<Material> materialList;
    private LinkedList<int> lastExpressions = new LinkedList<int>();
    public int ExpressionLevel { get; internal set; }

    public Cell(string label, List<Material> materialList)
    {
        this.labelString = label;
        //geneExpressions = new Dictionary<string, int> ();
        graphPoints = new List<GraphPoint>();
        this.materialList = materialList;
        attributes = new Dictionary<string, string>();
        facs = new Dictionary<string, int>();
    }

    public void SetLabel(string label)
    {
        labelString = label;
    }

    public void AddGraphPoint(GraphPoint g)
    {
        graphPoints.Add(g);
    }

    public void RemoveFromGraphs()
    {
        foreach (GraphPoint g in graphPoints)
        {
            g.gameObject.SetActive(!g.gameObject.activeSelf);
            //if (g.GetComponent<Rigidbody>() == null)
            //    g.gameObject.AddComponent<Rigidbody>();
        }
    }

    //public void SetExpressionData(string geneName, int colorSlot) {
    //	if (!geneExpressions.ContainsKey (geneName)) {
    //		geneExpressions.Add (geneName, colorSlot);
    //	}
    //}

    //public Material GetGeneMaterial(string geneName) {
    //	int colorSlot = 0;
    //	if (!geneExpressions.TryGetValue (geneName, out colorSlot)) {
    //		return null;
    //	} else {
    //		return materialList [colorSlot];
    //	}
    //}
    public string Label
    {
        get
        {
            return this.labelString;
        }
        set
        {
            this.labelString = value;
        }
    }



    public void ColorByAttribute(string attributeType, Color color)
    {
        if (attributes[attributeType] == "1")
        {
            foreach (GraphPoint g in graphPoints)
            {
                g.GetComponent<Renderer>().material.color = color;
            }
        }
    }

    public void AddAttribute(string attributeType, string attribute)
    {
        attributes[attributeType] = attribute;
    }

    public void ColorByPreviousExpression(int index)
    {
        LinkedListNode<int> node = lastExpressions.First;
        for (int i = 0; i < index; ++i)
        {
            node = node.Next;
        }

        int expression = node.Value;
        ExpressionLevel = expression;

        foreach (GraphPoint g in graphPoints)
        {
            if (expression > 29)
            {
                expression = 29;
            }
            g.GetComponent<Renderer>().material = materialList[expression];
        }
    }

    public void SaveExpression()
    {
        if (lastExpressions.Count == 10)
        {
            lastExpressions.RemoveLast();
        }
        lastExpressions.AddFirst(ExpressionLevel);
    }

    public void ColorByExpression(int expression)
    {
        ExpressionLevel = expression;

        foreach (GraphPoint g in graphPoints)
        {
            if (expression > 29)
            {
                expression = 29;
            }
            g.GetComponent<Renderer>().material = materialList[expression];
        }
    }

    public void ColorByIndex(string facsName)
    {
        foreach (GraphPoint g in graphPoints)
        {
            g.GetComponent<Renderer>().material = materialList[facs[facsName]];
        }
    }

    internal void AddFacs(string facsName, int index)
    {

        facs[facsName] = index;
    }
}
