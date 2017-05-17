using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellRemover : MonoBehaviour {

	ArrayList selectedCells = new ArrayList();

	void OnTriggerEnter(Collider other) {
		
		if (!selectedCells.Contains (other)) {
			selectedCells.Add (other);
		}
	}

	public void ConfirmRemove() {
		foreach (Collider other in selectedCells) {
			other.transform.parent = null;
			other.gameObject.AddComponent<Rigidbody>();
			other.attachedRigidbody.useGravity = true;
			other.attachedRigidbody.isKinematic = false;
			other.isTrigger = false;
		}
		selectedCells.Clear ();
	}
}
