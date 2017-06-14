using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Heatmap : MonoBehaviour {

	Dictionary<Cell, Color> containedCells;
	public CellManager cellManager;
	public GraphManager graphManager;
    public SelectionToolHandler selectionToolHandler;

	// private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
	// private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;

	// Use this for initialization
	void Start () {
		containedCells = new Dictionary<Cell, Color> ();
        ArrayList cells = selectionToolHandler.GetLastSelection();
        foreach (GraphPoint g in cells)
        {
            containedCells[g.GetCell()] = g.GetMaterial().color;
        }
        selectionToolHandler.HeatmapCreated();
	}

    public void ColorCells() {
		Graph[] graphs = graphManager.GetComponentsInChildren<Graph> ();
		foreach (Graph g in graphs) {
			GraphPoint[] points = g.GetComponentsInChildren<GraphPoint> ();
			// print ("found graphoints " + points.ToString());
			foreach (GraphPoint gp in points) {
				if (containedCells.ContainsKey (gp.GetCell ())) {
					//print ("found cell");
					gp.GetComponent<Renderer> ().material.color = containedCells [gp.GetCell ()];
				}

			}
		}
	}

}
