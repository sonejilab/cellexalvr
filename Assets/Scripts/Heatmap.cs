using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Heatmap : MonoBehaviour {

public Texture texture;
private Dictionary<Cell, Color> containedCells;
private GraphManager graphManager;
private SelectionToolHandler selectionToolHandler;

// Use this for initialization
void Start () {
	containedCells = new Dictionary<Cell, Color> ();
	ArrayList cells = selectionToolHandler.GetLastSelection();
	foreach (GraphPoint g in cells) {
		containedCells[g.GetCell()] = g.GetMaterial().color;
	}
	selectionToolHandler.HeatmapCreated();
}

public void UpdateImage(string filepath) {
	byte[] fileData = File.ReadAllBytes(filepath);
	Texture2D tex = new Texture2D(2, 2);
	tex.LoadImage(fileData);
	GetComponent<Renderer>().material.SetTexture("_MainTex", tex);
}

public void ColorCells() {
	Graph[] graphs = graphManager.GetComponentsInChildren<Graph> ();
	foreach (Graph g in graphs) {
		GraphPoint[] points = g.GetComponentsInChildren<GraphPoint> ();
		foreach (GraphPoint gp in points) {
			if (containedCells.ContainsKey (gp.GetCell ())) {
				gp.GetComponent<Renderer> ().material.color = containedCells [gp.GetCell ()];
			}

		}
	}
}

public void SetVars(GraphManager graphManager, SelectionToolHandler selectionToolHandler) {
	this.graphManager = graphManager;
	this.selectionToolHandler = selectionToolHandler;
}

}
