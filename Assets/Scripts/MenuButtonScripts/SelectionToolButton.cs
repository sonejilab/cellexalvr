using UnityEngine;

public class SelectionToolButton : MonoBehaviour
{
public TextMesh descriptionText;
public SelectionToolHandler selectionToolHandler;
public SteamVR_TrackedObject trackedObject;
public Sprite standardTexture;
public Sprite highlightedTexture;
public MenuController menuController;
public RotateMenu rotater;
public SelectionToolMenu selectionToolMenu;
private SteamVR_Controller.Device device;
private SpriteRenderer spriteRenderer;
private bool controllerInside = false;
private bool menuActive = false;
private bool buttonsInitialized = false;

void Start() {
	device = SteamVR_Controller.Input((int)trackedObject.index);
	spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
	spriteRenderer.sprite = standardTexture;
	//  highlightedTexture =
}

void Update() {
	if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
		// print("selection button");
		menuController.SwitchToOriginalModel();
		menuActive = !menuActive;
		selectionToolMenu.gameObject.SetActive(menuActive);
		selectionToolHandler.SetSelectionToolEnabled(menuActive);
		// selectionToolMenu.SetEnabledState(menuActive);
		if (menuActive && rotater.rotation == 0) {
			rotater.RotateLeft();
		}
		if (!buttonsInitialized) {
			selectionToolMenu.InitializeButtons();
			buttonsInitialized = true;
		}
		//if (!buttonsInitialized) {
		//buttonsInitialized = true;
		//}
	}
}

void OnTriggerEnter(Collider other) {
	if (other.gameObject.tag == "Controller") {
		descriptionText.text = "Toggle selection tool";
		spriteRenderer.sprite = highlightedTexture;
		controllerInside = true;
	}
}

void OnTriggerExit(Collider other) {
	if (other.gameObject.tag == "Controller") {
		descriptionText.text = "";
		spriteRenderer.sprite = standardTexture;
		controllerInside = false;
		//selectionToolHandler.SetSelectionToolEnabled(!selectionToolHandler.IsSelectionToolEnabled());
	}
}

}
