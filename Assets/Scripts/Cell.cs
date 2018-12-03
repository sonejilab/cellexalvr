using CellexalExtensions;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// Represents one cell. A cell may be present in multiple graphs.
/// </summary>
public class Cell
{
    public List<GraphPoint> GraphPoints;
    public List<CombinedGraph.CombinedGraphPoint> CombinedGraphPoints;

    public Dictionary<string, int> Attributes { get; private set; }
    public Dictionary<string, int> Facs { get; private set; }
    public int ExpressionLevel { get; internal set; }
    public string Label { get; set; }

    private GraphManager graphManager;
    private Dictionary<string, int> lastExpressions = new Dictionary<string, int>(16);
    private Dictionary<string, int[]> flashingExpressions = new Dictionary<string, int[]>();
    private Material tempMat;


    /// <summary>
    /// Creates a new cell.
    /// </summary>
    /// <param name="label"> A string that differentiates this cell from other cells. </param>
    /// <param name="graphManager"> The graphmanager that this cell has graphpoints in. </param>
    public Cell(string label, GraphManager graphManager)
    {
        this.graphManager = graphManager;
        this.Label = label;
        GraphPoints = new List<GraphPoint>();
        CombinedGraphPoints = new List<CombinedGraph.CombinedGraphPoint>();
        Attributes = new Dictionary<string, int>();
        Facs = new Dictionary<string, int>();
        tempMat = null;
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

    public void AddGraphPoint(CombinedGraph.CombinedGraphPoint g)
    {
        CombinedGraphPoints.Add(g);
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
    /// Colors all graphpoints that represents this cell if this cell is of an attribute.
    /// </summary>
    /// <param name="attributeType"> The attribute to color by. </param>
    /// <param name="color"> True if the graphpoints should be colored, false  if they should be white. (True means show this attribute, false means hide basically) </param>
    public void ColorByAttribute(string attributeType, bool color)
    {
        if (Attributes.ContainsKey(attributeType.ToLower()))
        {
            foreach (CombinedGraph.CombinedGraphPoint g in CombinedGraphPoints)
            {
                if (color)
                {
                    g.Recolor(graphManager.AttributeMaterials[Attributes[attributeType.ToLower()]].color);
                    //g.Material = graphManager.AttributeMaterials[Attributes[attributeType.ToLower()]];
                    //Debug.Log("ADD GROUP - " + Attributes[attributeType.ToLower()]);
                    //graphManager.referenceManager.selectionToolHandler.AddGraphpointToSelection(GraphPoints[0], Attributes[attributeType.ToLower()], false, g.Material.color);
                }
                    
                else
                {
                    //g.Material = graphManager.defaultGraphPointMaterial;
                    g.Recolor(graphManager.AttributeMaterials[Attributes[attributeType.ToLower()]].color);
                }
            }
        }
    }

    [Obsolete]
    public bool EvaluateAttributeLogic(Tuple<string, AttributeLogic>[] attributes)
    {
        foreach (var attribute in attributes)
        {
            string attributeName = attribute.Item1.ToLower();
            if (attribute.Item2 == AttributeLogic.YES && !Attributes.ContainsKey(attributeName))
                return false;
            if (attribute.Item2 == AttributeLogic.NO && Attributes.ContainsKey(attributeName))
                return false;
        }
        return true;
    }

    /// <summary>
    /// Color all graphpoints that represents this cells based on some boolean expression of attributes.
    /// </summary>
    /// <param name="attributes">The root of a tree representing a boolean expression of attribute.</param>
    public void ColorByAttributeLogic(BooleanExpression.Expr attributes)
    {
        if (attributes.Eval(this))
        {
            foreach (GraphPoint g in GraphPoints)
            {
                g.Material = graphManager.AttributeMaterials[0];
            }
        }
    }

    /// <summary>
    /// Adds an attribute to this cell.
    /// </summary>
    /// <param name="attributeType"> The type of the attribute. </param>
    /// <param name="color"> The color that should be used for this attribute. This corresponds to an index in <see cref="GraphManager.AttributeMaterials"/>. </param>
    public void AddAttribute(string attributeType, int color)
    {
        Attributes[attributeType.ToLower()] = color;
    }

    /// <summary>
    /// Colors this cell after a gene expression it was recently colored by.
    /// </summary>
    /// <param name="geneName"> The name of the gene </param>
    public void ColorByPreviousExpression(string geneName)
    {
        var expression = lastExpressions[geneName];
        ExpressionLevel = expression;


        foreach (GraphPoint g in GraphPoints)
        {
            if (expression > CellexalConfig.NumberOfExpressionColors - 1)
            {
                expression = CellexalConfig.NumberOfExpressionColors - 1;
            }
            g.Material = graphManager.GeneExpressionMaterials[expression];
        }
    }

    public void ResetColor()
    {
        foreach (GraphPoint g in GraphPoints)
        {
            g.ResetColor();
        }
    }

    /// <summary>
    /// Saves the current gene expression of this cell is colored by
    /// </summary>
    /// <param name="saveGeneName"> The genename to save </param>
    /// <param name="removeGeneName"> The name of a gene to remove or an empty string to not remove anything. Gene expressions can use up quite some memory so only 10 are saved at a time. </param>
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
    /// <param name="expression"> A number corresponding to an index in <see cref="GraphManager.GeneExpressionMaterials"/> of how expressed this cell is. </param>
    public void ColorByExpression(int expression)
    {
        ExpressionLevel = expression;

        foreach (GraphPoint g in GraphPoints)
        {
            if (expression >= CellexalConfig.NumberOfExpressionColors)
            {
                expression = CellexalConfig.NumberOfExpressionColors - 1;
            }

            tempMat = graphManager.GeneExpressionMaterials[expression];
            g.Material = g.transparentMaterial;
        }
    }

    /// <summary>
    /// Color all graphpoints that represents this cell by an index.
    /// I don't know enough biology to know what this actually is.
    /// </summary>
    /// <param name="facsName"> The index. </param>
    public void ColorByIndex(string facsName)
    {
        foreach (CombinedGraph.CombinedGraphPoint g in CombinedGraphPoints)
        {
            g.Recolor(graphManager.GeneExpressionMaterials[Facs[facsName.ToLower()]].color);
        }
    }

    /// <summary>
    /// Adds a .facs thing to this cell.
    /// </summary>
    /// <param name="facsName"> The thing's name. </param>
    /// <param name="index"> The value of the thing. </param>
    internal void AddFacs(string facsName, int index)
    {

        Facs[facsName.ToLower()] = index;
    }

    /// <summary>
    /// Sets the group and color of all graphpoints that are representing this cell.
    /// </summary>
    /// <param name="group"> The new group. </param>
    public void SetGroup(int group, bool changeColor)
    {
        foreach (GraphPoint g in GraphPoints)
        {
            g.CurrentGroup = group;
            if (changeColor)
                g.Material = graphManager.GroupingMaterials[group];
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
            g.Material = tempMat;
            var l = g.gameObject.GetComponent<ParticleSystem>();
            if (l != null)
            {
                GameObject.Destroy(l);
            }
        }
    }

    /// <summary>
    /// Turns all graphpoints representing this cell off.
    /// </summary>
    internal void Hide()
    {
        foreach (GraphPoint g in GraphPoints)
        {
            g.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Makes the graphpoints transparent. Used when showing the top expressed cells.
    /// </summary>
    internal void MakeTransparent()
    {
        foreach (GraphPoint g in GraphPoints)
        {
            g.Material = g.transparentMaterial;
        }
    }

    /// <summary>
    /// Highlights cells of choice to stick out among the others (e.g. top expressed ones).
    /// </summary>
    internal void Highlight()
    {
        foreach (GraphPoint g in GraphPoints)
        {
            g.Material = tempMat;

            // Highlighting via particle effects.
            ParticleSystem ps = g.gameObject.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startSize = 0.02f;
            main.startLifetime = 0.5f;
            main.startSpeed = 0;
            main.startColor = g.Material.color;
            main.maxParticles = 1;
            var shape = ps.shape;

            var renderer = g.gameObject.GetComponent<ParticleSystemRenderer>();
            renderer.material = g.particleMat;

            shape.enabled = false;

            // pulsating effect
            var sz = ps.sizeOverLifetime;
            sz.enabled = true;
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0.0f, 0.0f);
            curve.AddKey(0.5f, 1.0f);
            curve.AddKey(1.0f, 0.0f);
            sz.size = new ParticleSystem.MinMaxCurve(1.0f, curve);
        }
    }

    /// <summary>
    /// Initializes the cell for saving genee expressions for flashing.
    /// Should be called before <see cref="SaveSingleFlashingGenesExpression(string, int, int)"/>
    /// </summary>
    /// <param name="category">The name of a category that should be initialized</param>
    /// <param name="length">The number of genes in that category</param>
    public void InitSaveSingleFlashingGenesExpression(string category, int length)
    {
        flashingExpressions[category] = new int[length];
    }

    /// <summary>
    /// Saves a gene expression that can be flashed later.
    /// </summary>
    /// <param name="category">The name of the category that this gene is in</param>
    /// <param name="index">Which index it should be put on</param>
    /// <param name="expression">A value between 0 and <see cref="CellexalConfig.NumberOfExpressionColors"/></param>
    public void SaveSingleFlashingGenesExpression(string category, int index, int expression)
    {
        flashingExpressions[category][index] = expression;
    }

    /// <summary>
    /// Saves gene expressions so they can be flashed quickly later.
    /// </summary>
    /// <param name="category"> The category the gene expressions are in </param>
    /// <param name="expression"> An array containing indices corresponding to <see cref="GraphManager.GeneExpressionMaterials"/>. </param>
    public void SaveFlashingExpression(string category, int[] expression)
    {
        flashingExpressions[category] = expression;
    }

    /// <summary>
    /// Colors by a gene in a category. Used for flashing genes.
    /// </summary>
    /// <param name="category"> The name of the category to choose from. </param>
    /// <param name="index"> An index in from the array saved in <see cref="SaveFlashingExpression(string, int[])"/>. </param>
    public void ColorByGeneInCategory(string category, int index)
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
                g.Material = graphManager.GeneExpressionMaterials[expression];
            }
        }
        else
        {
            foreach (GraphPoint g in GraphPoints)
            {
                g.Material = graphManager.GeneExpressionMaterials[0];
            }
        }
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
