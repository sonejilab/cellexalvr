using UnityEngine;

///<summary>
/// This class represents a button used for resetting the input data folders.
///</summary>
public class ResetFolderButton : MonoBehaviour {

public TextMesh descriptionText;
public GraphManager graphManager;
public InputFolderGenerator inputFolderGenerator;
public LoaderController loader;
public SteamVR_TrackedObject rightController;
public Sprite standardTexture;
public Sprite highlightedTexture;
private SteamVR_Controller.Device device;
private SpriteRenderer spriteRenderer;
private bool controllerInside = false;
private bool menuActive = false;
private bool buttonsInitialized = false;

void Start() {
	device = SteamVR_Controller.Input((int)rightController.index);
	spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
	spriteRenderer.sprite = standardTexture;
}

void Update() {
	if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
		graphManager.DeleteGraphs();
		// must reset loader before generating new folders
		loader.ResetLoaderBooleans();
		inputFolderGenerator.GenerateFolders();
		if (loader.loaderMovedDown) {
			loader.loaderMovedDown = false;
			loader.MoveLoader(new Vector3(0f, 1f, 0f), 2f);
		}
	}
}

void OnTriggerEnter(Collider other) {
	if (other.gameObject.tag == "Controller") {
		descriptionText.text = "Reset folder";
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
