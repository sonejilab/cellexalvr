using UnityEngine;
using System.Collections.Generic;

public class MenuController : MonoBehaviour {

public GameObject originalModel;
public List<GameObject> activatedInMenu;
public List<GameObject> deactivatedInMenu;

void OnTriggerEnter(Collider other) {

	if (other.gameObject.tag == "Controller") {
		SetActivatedList(activatedInMenu, true);
		SetActivatedList(deactivatedInMenu, false);
	}
}

void OnTriggerExit(Collider other) {
	if (other.gameObject.tag == "Controller") {
		SetActivatedList(activatedInMenu, false);
		SetActivatedList(deactivatedInMenu, true);
		originalModel.GetComponent<SteamVR_RenderModel>().UpdateModel();
	}
}

public void SwichToOriginalModel() {
	SetActivatedList(activatedInMenu, false);
	SetActivatedList(deactivatedInMenu, true);
	originalModel.GetComponent<SteamVR_RenderModel>().UpdateModel();
}

void SetActivatedList(List<GameObject> list, bool activated) {
	foreach(GameObject item in list) {
		item.SetActive(activated);
	}
}

}
