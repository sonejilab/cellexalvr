using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellSelector : MonoBehaviour {

	public GraphManager manager;
	public Graph graph;
	public Material selectorMaterial;

	ArrayList selectedCells = new ArrayList();
	Color[] colors;
	int currentColorIndex = 0;
	Color selectedColor;

	public void Start() {
		colors = new Color[6];
		colors [0] = new Color (1, 0, 0); // red
		colors [1] = new Color (0, 0, 1); // blue
		colors [2] = new Color (0, 1, 1); // cyan
		colors [3] = new Color (1, 0, 1); // magenta
		colors [4] = new Color (1f, 153f/255f, 204f/255f); // pink
		colors [5] = new Color (255, 255, 0); // yellow
		// colors [4] = new Color (1, 0.92, 0.016, 1); // yellow
	
		selectorMaterial.color = colors [0];

		selectedColor = Color.red;
		// print (selectedColor.ToString ());
	}

	void OnTriggerEnter(Collider other) {
		other.GetComponentInChildren<Renderer> ().material.color = selectedColor;
		selectedCells.Add(other);
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
			cell.GetComponentInChildren<Renderer> ().material.color = Color.green;
		}
		// clear the list since we are done with it
		selectedCells.Clear ();

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


}
