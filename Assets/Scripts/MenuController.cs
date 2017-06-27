using UnityEngine;
using System.Collections.Generic;

public class MenuController : MonoBehaviour {

public SteamVR_RenderModel originalModel;
public List<GameObject> activatedInMenu;
public List<GameObject> deactivatedInMenu;

void OnTriggerEnter(Collider other) {
	if (other.gameObject.tag == "Controller") {
		SetActivatedList(activatedInMenu, true);
		SetActivatedList(deactivatedInMenu, false);
		// originalModel.enabled = false;
		originalModel.gameObject.SetActive(false);
		// originalModel.UpdateModel();
	}
}

void OnTriggerExit(Collider other) {
	if (other.gameObject.tag == "Controller") {
		SetActivatedList(activatedInMenu, false);
		SetActivatedList(deactivatedInMenu, true);
		// originalModel.enabled = true;
		originalModel.gameObject.SetActive(true);
		originalModel.UpdateModel();
	}
}

public void SwitchToOriginalModel() {
	SetActivatedList(activatedInMenu, false);
	SetActivatedList(deactivatedInMenu, true);
	// originalModel.enabled = true;
	originalModel.gameObject.SetActive(true);
	originalModel.UpdateModel();
}

void SetActivatedList(List<GameObject> list, bool activated) {
	foreach(GameObject item in list) {
		item.SetActive(activated);
	}
}

}
