using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellSelector : MonoBehaviour {

	ArrayList selectedCells = new ArrayList();

	void OnTriggerEnter(Collider other) {
		other.GetComponent<Renderer> ().material.color = Color.red;
		selectedCells.Add(other);
	}

	public void RemoveCells () {
		foreach(Collider cell in selectedCells) {
			cell.attachedRigidbody.useGravity = true;
			cell.isTrigger = false;
		}

	}
}
