using UnityEngine;

public class SelectionToolCollider : MonoBehaviour {

public SelectionToolHandler selectionToolHandler;

void OnTriggerEnter(Collider other) {
	selectionToolHandler.Trigger(other);
}
}
