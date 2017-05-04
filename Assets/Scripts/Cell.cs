using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Cell {
	private string label;
	private Dictionary<string, int> geneExpressions;
	private List<Material> materialList;

	public Cell (string label, List<Material> materialList) {
		this.label = label;
		geneExpressions = new Dictionary<string, int> ();
		this.materialList = materialList;

	}

	public void setLabel(string label)
	{
		this.label = label;
	}

	public void setExpressionData(string geneName, int colorSlot){
		geneExpressions.Add (geneName, colorSlot);
	}

	public Material getGeneMaterial(string geneName) {
		int colorSlot = 0;
		geneExpressions.TryGetValue (geneName, out colorSlot);
		return materialList [colorSlot];

	}
}
