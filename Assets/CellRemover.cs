using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellRemover : MonoBehaviour {

	void OnTriggerEnter(Collider other) {
		other.attachedRigidbody.useGravity = true;
		// other.attachedRigidbody.isKinematic = true;
		other.isTrigger = false;
	}
}
