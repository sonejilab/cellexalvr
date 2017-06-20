using UnityEngine;

public class ResetGraphButton : MonoBehaviour {
public TextMesh descriptionText;
public SteamVR_TrackedObject trackedObject;
public Sprite standardTexture;
public Sprite highlightedTexture;
public GraphManager graphManager;
private SteamVR_Controller.Device device;
private bool controllerInside;
private SpriteRenderer spriteRenderer;

// Use this for initialization
void Start() {
	device = SteamVR_Controller.Input((int)trackedObject.index);
	spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
	spriteRenderer.sprite = standardTexture;
//  highlightedTexture =
}

// Update is called once per frame
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
		//selectionToolHandler.SetSelectionToolEnabled(!selectionToolHandler.IsSelectionToolEnabled());
	}

}

}
