using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using VRTK;

public class CellSelector : MonoBehaviour {

	public GraphManager manager;
	public Graph graph;
	public Material selectorMaterial;
	public RadialMenu menu;
	public Sprite confirmButton;
	public Sprite noConfirmButton;


	List<RadialMenuButton> buttons;
	bool showConfirmButton = false;

	bool selectionDone = false;
	ArrayList selectedCells = new ArrayList();
	Color[] colors;
	int currentColorIndex = 0;
	Color selectedColor;
	int fileCreationCtr = 0;

	public void Start() {
		colors = new Color[10];
		colors [0] = new Color (1, 0, 0); // red
		colors [1] = new Color (0, 0, 1); // blue
		colors [2] = new Color (0, 1, 1); // cyan
		colors [3] = new Color (1, 0, 1); // magenta
		colors [4] = new Color (1f, 153f/255f, 204f/255f); // pink
		colors [5] = new Color (1, 1, 0); // yellow
		colors [6] = new Color (0, 0, 1); // green
		colors [7] = new Color (.6f, 1, .6f); // lime green
		colors [8] = new Color (.4f, .2f, 1); // brown
		colors [9] = new Color (1, .6f, .2f); // orange
		// colors [4] = new Color (1, 0.92, 0.016, 1); // yellow
	
		selectorMaterial.color = colors [0];

		selectedColor = Color.red;
		// print (selectedColor.ToString ());

		// get the radial menue from the right hand control
		buttons = menu.buttons;

		buttons [0].ButtonIcon = noConfirmButton;

	}

	void OnTriggerEnter(Collider other) {
		if (selectionDone) {
			selectedCells.Clear ();
			menu.RegenerateButtons();
			selectionDone = false;
		}
		if (other.GetComponentInChildren<Renderer> ().material.color != null) {
			other.GetComponentInChildren<Renderer> ().material.color = selectedColor;
		} else {
			print ("null color, Cellselector.cs:60");
		}
		if (!selectedCells.Contains (other)) {
			selectedCells.Add (other);
		}
		print ("OnTriggerEnter");
		buttons [0].ButtonIcon = confirmButton;
		if(!showConfirmButton) {
			showConfirmButton = true;
			menu.RegenerateButtons();
		}



		//menu.buttons[0].ButtonIcon = confirmButton;
			

	}

	public void RemoveCells () {
		foreach(Collider cell in selectedCells) {
			cell.attachedRigidbody.useGravity = true;
			cell.isTrigger = false;
		}
	}

	public void ConfirmSelection () {
		Graph newGraph = Instantiate (graph);
		newGraph.gameObject.SetActive (true);
		newGraph.transform.parent = manager.transform;
		foreach(Collider cell in selectedCells) {
			GameObject graphpoint = cell.gameObject;
			graphpoint.transform.parent = newGraph.transform;
			Color c = cell.GetComponentInChildren<Renderer> ().material.color;
			Color transparentVersion = new Color (c.r, c.g, c.b, .3f);
			cell.GetComponentInChildren<Renderer> ().material.color = transparentVersion;

		}
		// clear the list since we are done with it
		// ?

		// create .txt file with latest selection
		dumpData();

		selectionDone = true;

		print ("ConfirmSelection");
		buttons [0].ButtonIcon = noConfirmButton;
		menu.RegenerateButtons();
		showConfirmButton = false;

	}
		
	public void ChangeColor() {
		if (currentColorIndex == colors.Length - 1) {
			currentColorIndex = 0;
		} else {
			currentColorIndex++;
		}
		selectedColor = colors [currentColorIndex];
		selectorMaterial.color = selectedColor;
	}

	public void ctrlZ()
	{
		selectedCells.RemoveAt(selectedCells.Count - 1);
	}

	public void dumpData()
	{
		using (System.IO.StreamWriter file =
			new System.IO.StreamWriter(Directory.GetCurrentDirectory() + "\\Assets\\Data\\runtimeGroups\\selection" + fileCreationCtr++ + ".txt")) {
			// new System.IO.StreamWriter(Directory.GetCurrentDirectory() + "\\Assets\\Data\\runtimeGroups\\" + DateTime.Now.ToShortTimeString() + ".txt"))

			// print ("dumping data");
			foreach (Collider cell in selectedCells)
			{
				file.Write(cell.GetComponent<GraphPoint>().getLabel());
				file.Write ("\t");
				Color c = cell.GetComponentInChildren<Renderer> ().material.color;
				int r = (int)(c.r * 255);
				int g = (int)(c.g * 255);
				int b = (int)(c.b * 255);
				file.Write(string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b));
				file.WriteLine ();
				//print ("wrote " + cell.GetComponent<GraphPoint> ().getLabel () + "\t" + string.Format("#{0:X2}{1:X2}{2:X2}", r, g, b));
			}
			file.Flush ();
			file.Close ();
		}
	}

}
/*
 * change tool
 * change colour
 * select/confirm
 * */