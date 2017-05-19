using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Cell {
	private string labelString;
	private Dictionary<string, int> geneExpressions;
	private List<Material> materialList;

	public Cell (string label, List<Material> materialList) {
		this.labelString = label;
		geneExpressions = new Dictionary<string, int> ();
		this.materialList = materialList;

	}

	public void setLabel(string label)
	{
		this.labelString = label;

	}

	public void setExpressionData(string geneName, int colorSlot){
		if (!geneExpressions.ContainsKey (geneName)) {
			geneExpressions.Add (geneName, colorSlot);
		}
	}

	public Material getGeneMaterial(string geneName) {
		int colorSlot = 0;
		if (!geneExpressions.TryGetValue (geneName, out colorSlot)) {
			return null;
		} else {
			return materialList [colorSlot];
		}
	}

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
}
