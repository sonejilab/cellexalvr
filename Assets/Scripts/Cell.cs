using System;
using System.Collections.Generic;
using UnityEngine;

public class Cell
{

    private string labelString;
    //private Dictionary<string, int> geneExpressions;
    private List<GraphPoint> graphPoints;
    private List<Material> materialList;

    public Cell(string label, List<Material> materialList)
    {
        this.labelString = label;
        //geneExpressions = new Dictionary<string, int> ();
        graphPoints = new List<GraphPoint>();
        this.materialList = materialList;
    }

    public void SetLabel(string label)
    {
        this.labelString = label;
    }

    public void AddGraphPoint(GraphPoint g)
    {
        graphPoints.Add(g);
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

    public void ColorByExpression(int expression)
    {
        foreach (GraphPoint g in graphPoints)
        {
            //if (expression == 0)
            //{
            //    Debug.Log(0);
            //}
            if (expression > 29)
            {
                //Debug.Log("array index out of bounds in " + labelString + " with " + expression);
                expression = 29;
            }
            g.GetComponent<Renderer>().material = materialList[expression];
        }
    }
}
