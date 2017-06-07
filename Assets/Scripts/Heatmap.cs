using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class Heatmap : MonoBehaviour {

	Dictionary<Cell, Color> containedCells;
	public CellManager cellManager;
	public GraphManager graphManager;

	private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
	private Valve.VR.EVRButtonId gripButton = Valve.VR.EVRButtonId.k_EButton_Grip;

	// Use this for initialization
	void Start () {
		containedCells = new Dictionary<Cell, Color> ();
		string home = Directory.GetCurrentDirectory();
		string fileName = home + "\\Assets\\Data\\runtimeGroups\\selection" + (SelectionToolHandler.fileCreationCtr - 1) + ".txt";
		print (fileName);
		readFile (fileName);
	}


	void readFile(string fileName) {
		string[] lines = System.IO.File.ReadAllLines(fileName);
		foreach (string line in lines) {
			// the coordinates are split with tab characters
			string[] words = line.Split('\t');
			Color color;
			ColorUtility.TryParseHtmlString (words [1], out color);
			containedCells.Add (cellManager.getCell (words [0]), color);
		}

	}

	public void colorCells() {
		Graph[] graphs = graphManager.GetComponentsInChildren<Graph> ();
		foreach (Graph g in graphs) {
			GraphPoint[] points = g.GetComponentsInChildren<GraphPoint> ();
			print ("found graphoints " + points.ToString());
			foreach (GraphPoint gp in points) {
				if (containedCells.ContainsKey (gp.getCell ())) {
					print ("found cell");
					gp.GetComponentInChildren<Renderer> ().material.color = containedCells [gp.getCell ()];
				}
				
			}
		}
	}


}
