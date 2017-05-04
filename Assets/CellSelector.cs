using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellSelector : MonoBehaviour {

	public Graph graph;

	ArrayList selectedCells = new ArrayList();
	Color[] colors;
	int currentColorIndex = 0;
	Color selectedColor;

	public void Start() {
		colors = new Color[4];
		colors [0] = new Color (1, 0, 0); // red
		colors [1] = new Color (0, 0, 1); // blue
		colors [2] = new Color (0, 1, 1); // cyan
		colors [3] = new Color (1, 0, 1); // magenta
		// colors [4] = new Color (1, 0.92, 0.016, 1); // yellow

		selectedColor = Color.red;
		print (selectedColor.ToString ());
	}

	void OnTriggerEnter(Collider other) {
		other.GetComponent<Renderer> ().material.color = selectedColor;
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
		foreach(Collider cell in selectedCells) {
			GameObject graphpoint = cell.transform.parent.gameObject;
			graphpoint.transform.parent = newGraph.transform;
			cell.GetComponent<Renderer> ().material.color = Color.green;
		}

	}
		
	public void ChangeColor() {
		if (currentColorIndex == colors.Length - 1) {
			currentColorIndex = 0;
		} else {
			currentColorIndex++;
		}
		selectedColor = colors [currentColorIndex];
	}


}
