using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This class is responsible for changing th econtroller models when they go too close to the menu.
/// </summary>
public class MenuController : MonoBehaviour {

public SteamVR_RenderModel originalModel;
public SelectionToolHandler selectionToolHandler;
public GameObject fire;
public List<GameObject> activatedInMenu;
public List<GameObject> deactivatedInMenu;
private bool selectionToolEnabled = false;
private bool fireEnabled = false;

void OnTriggerEnter(Collider other) {
	if (other.gameObject.tag == "Controller") {
		SwitchToMenuModel();
		selectionToolEnabled = selectionToolHandler.IsSelectionToolEnabled();
		selectionToolHandler.SetSelectionToolEnabled(false);
		fireEnabled = fire.activeSelf;
		fire.SetActive(false);

	}
}

void OnTriggerExit(Collider other) {
	if (other.gameObject.tag == "Controller") {
		SwitchToOriginalModel();
		if (selectionToolEnabled) {
			selectionToolHandler.SetSelectionToolEnabled(true);
		}
		if (fireEnabled) {
			fire.SetActive(true);
		}
	}
}

/// <summary>
/// Should be called when a button that changes the tool is pressed.
/// </summary>
public void ToolSwitched() {
	selectionToolEnabled = false;
	fireEnabled = false;
}

/// <summary>
/// Switches the controller to the original model.
/// </summary>
public void SwitchToOriginalModel() {
	SetActivatedList(activatedInMenu, false);
	SetActivatedList(deactivatedInMenu, true);
	originalModel.gameObject.SetActive(true);
	originalModel.UpdateModel();
}

/// <summary>
/// Switrches the controller to the model that should be used when pressing buttons on the menu.
/// </summary>
public void SwitchToMenuModel() {
	SetActivatedList(activatedInMenu, true);
	SetActivatedList(deactivatedInMenu, false);
	originalModel.gameObject.SetActive(false);
}

void SetActivatedList(List<GameObject> list, bool activated) {
	foreach(GameObject item in list) {
		item.SetActive(activated);
	}
}

/// <summary>
/// Shows or hides the menu.
/// </summary>
public void ShowMenu(bool show) {
	foreach (Renderer r in GetComponentsInChildren<Renderer>()) {
		r.enabled = show;
	}
	foreach (Collider c in GetComponentsInChildren<Collider>()) {
		c.enabled = show;
	}
}

/// <summary>
/// Check if the contoller is inside the menu. If, switch to the menu model. If not, switch to the normal model.
/// </summary>
public void SwitchControllerModel() {
	BoxCollider collider = gameObject.GetComponent<BoxCollider>();
	Vector3 center = collider.transform.position + collider.center;
	Vector3 halfExtents = Vector3.Scale(collider.size / 2, collider.transform.localScale);
	int layerMask = 0;
	layerMask = ~layerMask;
	Quaternion rotation = collider.transform.rotation;
	Collider[] allOverlappingColliders = Physics.OverlapBox(center, halfExtents, rotation, layerMask, QueryTriggerInteraction.Collide);
	bool controllerInside = false;
	foreach (Collider c in allOverlappingColliders) {
		if (c.gameObject.tag == "Controller") {
			SwitchToMenuModel();
			controllerInside = true;
			break;
		}
	}
	if (!controllerInside) {
		SwitchToOriginalModel();
	}

}

}
