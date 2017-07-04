namespace VRTK.GrabAttachMechanics
{

using UnityEngine;

/// <summary>
/// This class is used with VRTK's interaction system when a heatmap is grabbed.
/// It hides the menu and any tool active while the heatmap is grabbed.
/// </summary>
public class HeatmapGrab : VRTK_BaseJointGrabAttach {

public float breakForce = 1500f;
GameObject menu;
private MenuController menuController;
private SelectionToolHandler selectionToolHandler;
private bool menuTurnedOff = false;
private bool selectionToolTurnedOff = false;
void Start() {
	// finds the menu, even if it is turned off
	menuController = Resources.FindObjectsOfTypeAll<MenuController>()[0];
	menu = menuController.gameObject;
	selectionToolHandler = Resources.FindObjectsOfTypeAll<SelectionToolHandler>()[0];
	// this really isn't the right way of doing this
	foreach (SelectionToolHandler s in Resources.FindObjectsOfTypeAll<SelectionToolHandler>()) {
		if (s.gameObject.transform.parent != null) {
			if (s.gameObject.transform.parent.name == "Controller (right)") {
				selectionToolHandler = s;
			}
		}
	}
}

protected override void CreateJoint(GameObject obj) {
	if (menu.activeSelf) {
		menuTurnedOff = true;
		menu.SetActive(false);
		menuController.SwitchToOriginalModel();
	} else {
		menuTurnedOff = false;
	}
	selectionToolTurnedOff = selectionToolHandler.IsSelectionToolEnabled();
	if(selectionToolTurnedOff) {
		selectionToolHandler.SetSelectionToolEnabled(false);
	}
	givenJoint = obj.AddComponent<FixedJoint>();
	givenJoint.breakForce = (grabbedObjectScript.IsDroppable() ? breakForce : Mathf.Infinity);
	base.CreateJoint(obj);
}

protected override void DestroyJoint(bool withDestroyImmediate, bool applyGrabbingObjectVelocity) {
	base.DestroyJoint(withDestroyImmediate, applyGrabbingObjectVelocity);
	if (selectionToolTurnedOff) {
		selectionToolHandler.SetSelectionToolEnabled(true);
	}
	menu.SetActive(true);
	if (!menuTurnedOff) {
		menu.SetActive(false);
	} else {
		menuController.SwitchControllerModel();
	}
}

}

}
