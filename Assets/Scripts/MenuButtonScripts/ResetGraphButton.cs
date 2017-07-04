using UnityEngine;

///<summary>
/// This class represents a button used for resetting the color and position of the graphs.
///</summary>
public class ResetGraphButton : MonoBehaviour {
public TextMesh descriptionText;
public SteamVR_TrackedObject rightController;
public Sprite standardTexture;
public Sprite highlightedTexture;
public GraphManager graphManager;
private SteamVR_Controller.Device device;
private bool controllerInside;
private SpriteRenderer spriteRenderer;

void Start() {
	device = SteamVR_Controller.Input((int)rightController.index);
	spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
	spriteRenderer.sprite = standardTexture;
}

void Update() {
	if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
		graphManager.ResetGraph();
	}
}

void OnTriggerEnter(Collider other) {
	if (other.gameObject.tag == "Controller") {
		descriptionText.text = "This button resets the graphs";
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
