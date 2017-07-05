using UnityEngine;

/// <summary>
/// This class sole purpose is to forward collision events to the selection tool handler
/// </summary>
public class SelectionToolCollider : MonoBehaviour {

public SelectionToolHandler selectionToolHandler;

void OnTriggerEnter(Collider other) {
	selectionToolHandler.Trigger(other);
}
}
