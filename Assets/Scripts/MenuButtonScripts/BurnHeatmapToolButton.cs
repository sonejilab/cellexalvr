using UnityEngine;

///<summary>
/// This class represents a button used for toggling the burning heatmap tool.
///</summary>
public class BurnHeatmapToolButton : MonoBehaviour {
public TextMesh descriptionText;
public GameObject fire;
public SteamVR_TrackedObject rightController;
public Sprite standardTexture;
public Sprite highlightedTexture;
public ControllerModelSwitcher menuController;
private SteamVR_Controller.Device device;
private SpriteRenderer spriteRenderer;
private bool controllerInside = false;
private bool fireActivated = false;

void Start() {
	device = SteamVR_Controller.Input((int)rightController.index);
	spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
	spriteRenderer.sprite = standardTexture;
}

void Update() {
	if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
		fireActivated = !fireActivated;
		fire.SetActive(fireActivated);
		menuController.ToolSwitched();
	}
}

void OnTriggerEnter(Collider other) {
	if (other.gameObject.tag == "Controller") {
		descriptionText.text = "Burn heatmap tool";
		spriteRenderer.sprite = highlightedTexture;
		controllerInside = true;
	}
}

void OnTriggerExit(Collider other) {
	if (other.gameObject.tag == "Controller") {
		descriptionText.text = "";
		spriteRenderer.sprite = standardTexture;
		controllerInside = false;
	}
}

}
