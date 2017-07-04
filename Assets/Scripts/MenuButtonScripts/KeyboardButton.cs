using UnityEngine;

///<summary>
/// This class represents a button used for toggling the keyboard.
///</summary>
public class KeyboardButton : MonoBehaviour {
public TextMesh descriptionText;
public SteamVR_TrackedObject rightController;
public Sprite standardTexture;
public Sprite highlightedTexture;
public GameObject keyboard;
public VRTK.VRTK_StraightPointerRenderer laserPointer;
private SteamVR_Controller.Device device;
private bool controllerInside;
private SpriteRenderer spriteRenderer;
private bool keyboardActivated = false;

void Awake() {
	keyboard.SetActive(false);
}

void Start() {
	device = SteamVR_Controller.Input((int)rightController.index);
	spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
	spriteRenderer.sprite = standardTexture;
}

void Update() {
	if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger)) {
		keyboardActivated = !keyboardActivated;
		laserPointer.enabled = keyboardActivated;
		keyboard.SetActive(keyboardActivated);
	}
}

void OnTriggerEnter(Collider other) {
	if (other.gameObject.tag == "Controller") {
		descriptionText.text = "Toggle keyboard for\ncoloring by gene";
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
