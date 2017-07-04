using UnityEngine;

///<summary>
/// This class represents a button used the graphs from the cell selection used for this particular heatmap.
///</summary>
public class RecolourHeatmapButton : MonoBehaviour {
private TextMesh descriptionText;
public SteamVR_TrackedObject rightController;
public Sprite standardTexture;
public Sprite highlightedTexture;
private SteamVR_Controller.Device device;
private SpriteRenderer spriteRenderer;
private bool controllerInside = false;

void Start() {
	rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
	device = SteamVR_Controller.Input((int)rightController.index);
	spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
	spriteRenderer.sprite = standardTexture;
	descriptionText = GetComponentInChildren<TextMesh>();
}

void Update() {
	if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
		//print(gameObject.GetComponentInParent<Heatmap>().gameObject.name);
		gameObject.GetComponentInParent<Heatmap>().ColorCells();
	}
}

void OnTriggerEnter(Collider other) {
	if (other.gameObject.tag == "Controller") {
		descriptionText.text = "Recolour graphs";
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
