using UnityEngine;

public class SampleMenuButtonScript : MonoBehaviour {
public TextMesh descriptionText;
public SteamVR_TrackedObject rightController;
public Sprite standardTexture;
public Sprite highlightedTexture;
private SteamVR_Controller.Device device;
private bool controllerInside;
private SpriteRenderer spriteRenderer;

// Use this for initialization
void Start() {
	device = SteamVR_Controller.Input((int)rightController.index);
	spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
	spriteRenderer.sprite = standardTexture;
//  highlightedTexture =
}

// Update is called once per frame
void Update() {
	if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
		// do something
	}
}

void OnTriggerEnter(Collider other) {
	if (other.gameObject.CompareTag("Smaller Controller Collider")) {
		descriptionText.text = "This button does nothing";
		spriteRenderer.sprite = highlightedTexture;
		controllerInside = true;
	}
}

void OnTriggerExit(Collider other) {
	if (other.gameObject.CompareTag("Smaller Controller Collider")) {
		descriptionText.text = "";
		spriteRenderer.sprite = standardTexture;
		controllerInside = false;
		//selectionToolHandler.SetSelectionToolEnabled(!selectionToolHandler.IsSelectionToolEnabled());
	}

}

}
