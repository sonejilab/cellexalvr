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
    private Dictionary<string, int> lastExpressions = new Dictionary<string, int>(16);
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

    /// <summary>
    /// Tell this cell that it is now represented by a graphpoint.
    /// A cell may be represented by many graphpoints (typically one in each graph).
    /// </summary>
    /// <param name="g"> The graphpoint representing this cell. </param>
    public void AddGraphPoint(GraphPoint g)
    {
        graphPoints.Add(g);
    }

    /// <summary>
    /// Turns off all graphpoints that this cell is represented by.
    /// </summary>
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


    /// <summary>
    /// Color this cell by an attribute, if it is of that attribute.
    /// </summary>
    /// <param name="attributeType"> The attribute to color by. </param>
    /// <param name="color"> The color to give the graphpoints. </param>
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

    /// <summary>
    /// Adds an attribute to this cell.
    /// </summary>
    /// <param name="attributeType"> The type of the attribute. </param>
    /// <param name="value"> "1" if this cell is of that attribute, "0" otherwise. </param>
    public void AddAttribute(string attributeType, string value)
    {
        attributes[attributeType] = value;
    }

    /// <summary>
    /// Colors this cell after a gene expression it was recently colored by.
    /// </summary>
    /// <param name="index"> The index of the gene (from the list of the 10 last genes) </param>
    public void ColorByPreviousExpression(string geneName)
    {
        var expression = lastExpressions[geneName];
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

    /// <summary>
    /// Saves the current gene expression of this cell is colored by
    /// </summary>
    public void SaveExpression(string saveGeneName, string removeGeneName)
    {
        if (removeGeneName != null && removeGeneName != "")
        {
            lastExpressions.Remove(removeGeneName);
        }
        lastExpressions[saveGeneName] = ExpressionLevel;
    }

    /// <summary>
    /// Color all graphpoints that represents this cell by their expression of a gene.
    /// </summary>
    /// <param name="expression"> A number [0, 29] of how expressed this cell is. </param>
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

    /// <summary>
    /// Color all graphpoints that represents this cell by an index.
    /// I don't know enough biology to know what this actually is.
    /// </summary>
    /// <param name="facsName"> The index. </param>
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
