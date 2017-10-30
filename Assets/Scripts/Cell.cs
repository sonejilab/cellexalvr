using System;
using System.Collections.Generic;
using SQLiter;
using UnityEngine;

/// <summary>
/// This class represents one cell. A cell may be present in multiple graphs.
/// </summary>
public class Cell
{
    public List<GraphPoint> GraphPoints;

    private string labelString;
    private Dictionary<string, string> attributes;
    private Dictionary<string, int> facs;
    private List<Material> materialList;
    private Dictionary<string, int> lastExpressions = new Dictionary<string, int>(16);
    private Dictionary<string, int[]> flashingExpressions = new Dictionary<string, int[]>();
    public int ExpressionLevel { get; internal set; }

    public string Label
    {
        get { return labelString; }
        set { labelString = value; }
    }


    /// <summary>
    /// Creates a new cell.
    /// </summary>
    /// <param name="label"> A string that differentiates this cell from other cells. </param>
    /// <param name="materialList"> A list of materials that should be used when coloring. </param>
    public Cell(string label, List<Material> materialList)
    {
        this.labelString = label;
        GraphPoints = new List<GraphPoint>();
        this.materialList = materialList;
        attributes = new Dictionary<string, string>();
        facs = new Dictionary<string, int>();
    }

    /// <summary>
    /// Tell this cell that it is now represented by a graphpoint.
    /// A cell may be represented by many graphpoints (typically one in each graph).
    /// </summary>
    /// <param name="g"> The graphpoint representing this cell. </param>
    public void AddGraphPoint(GraphPoint g)
    {
        GraphPoints.Add(g);
    }

    /// <summary>
    /// Toggles all graphpoints that this cell is represented by.
    /// </summary>
    public void ToggleGraphPoints()
    {
        foreach (GraphPoint g in GraphPoints)
        {
            g.gameObject.SetActive(!g.gameObject.activeSelf);
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
            foreach (GraphPoint g in GraphPoints)
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


        foreach (GraphPoint g in GraphPoints)
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

        foreach (GraphPoint g in GraphPoints)
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
        foreach (GraphPoint g in GraphPoints)
        {
            g.GetComponent<Renderer>().material = materialList[facs[facsName]];
        }
    }

    /// <summary>
    /// Adds a .facs thing to this cell.
    /// </summary>
    /// <param name="facsName"> The thing's name. </param>
    /// <param name="index"> The value of the thing. </param>
    internal void AddFacs(string facsName, int index)
    {

        facs[facsName] = index;
    }

    /// <summary>
    /// Sets the group and color of all graphpoints that are representing this cell.
    /// </summary>
    /// <param name="group"> The new group. </param>
    public void SetGroup(Color col, int group)
    {
        foreach (GraphPoint g in GraphPoints)
        {
            g.Color = col;
            g.CurrentGroup = group;
        }
    }

    /// <summary>
    /// Turns all graphpoints representing this cell on.
    /// </summary>
    internal void Show()
    {
        foreach (GraphPoint g in GraphPoints)
        {
            g.gameObject.SetActive(true);
        }
    }

    public void SaveFlashingExpression(string category, int[] expression)
    {
        flashingExpressions[category] = expression;
    }

    public bool ColorByGeneInCategory(string category, int index)
    {
        if (flashingExpressions.ContainsKey(category))
        {
            int expression = flashingExpressions[category][index];
            foreach (GraphPoint g in GraphPoints)
            {
                if (expression > 29)
                {
                    expression = 29;
                }
                g.GetComponent<Renderer>().material = materialList[expression];
            }
        }
        else
        {
            foreach (GraphPoint g in GraphPoints)
            {
                g.GetComponent<Renderer>().material = materialList[0];
            }
        }
        return true;
    }

    /// <summary>
    /// Gets the lengths of each category.
    /// </summary>
    /// <returns> A Dictionary with the categories as keys and their lengths as values. </returns>
    internal Dictionary<string, int> GetCategoryLengths()
    {
        Dictionary<string, int> lengths = new Dictionary<string, int>();
        foreach (KeyValuePair<string, int[]> pair in flashingExpressions)
        {
            lengths[pair.Key] = pair.Value.Length;
        }
        return lengths;
    }

    /// <summary>
    /// Clears the saved flashing expressions.
    /// </summary>
    public void ClearFlashingExpressions()
    {
        flashingExpressions.Clear();
    }
}
