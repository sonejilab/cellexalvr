using UnityEngine;
using System.Collections;

public abstract class RotatableButton : MonoBehaviour {

public TextMesh descriptionText;
public SteamVR_TrackedObject trackedObject;
public Sprite standardTexture;
public Sprite highlightedTexture;
// all buttons must override this variable's get property
abstract protected string description {
	get;
}
protected SteamVR_Controller.Device device;
protected bool controllerInside;
private SpriteRenderer frontsideRenderer;
private SpriteRenderer backsideRenderer;
private Collider buttonCollider;
protected bool isRotating = false;
private bool isActivated = true;

void Start() {
	device = SteamVR_Controller.Input((int)trackedObject.index);
	frontsideRenderer = gameObject.GetComponent<SpriteRenderer>();
	frontsideRenderer.sprite = standardTexture;
	backsideRenderer = gameObject.GetComponentsInChildren<SpriteRenderer>()[1];
	buttonCollider = gameObject.GetComponent<Collider>();
}

void OnTriggerEnter(Collider other) {
	if (other.gameObject.tag == "Controller") {
		descriptionText.text = description;
		frontsideRenderer.sprite = highlightedTexture;
		controllerInside = true;
	}
}

void OnTriggerExit(Collider other) {
	if (other.gameObject.tag == "Controller") {
		// if the controller has moved directly to another button without
		// exiting this button's collider the other button will have changed the
		// text and we shouldn't mess with it
		if (descriptionText.text == description) {
			descriptionText.text = "";
		}
		frontsideRenderer.sprite = standardTexture;
		controllerInside = false;
	}
}

public void SetButtonState(bool active) {
	if (isActivated != active) {
		isActivated = active;
		if (active) {
			StartCoroutine(FlipButtonRoutine(180f, 0.15f, active));
		} else {
			// no need to keep the description up if we are deactivating the button
			descriptionText.text = "";
			StartCoroutine(FlipButtonRoutine(-180f, 0.15f, active));
		}
	}
}

IEnumerator FlipButtonRoutine(float yAngles, float inTime, bool active) {
	controllerInside = false;
	buttonCollider.enabled = false;
	frontsideRenderer.enabled = true;
	backsideRenderer.enabled = true;
	isRotating = true;
	// how much we have rotated so far
	float rotatedTotal = 0;
	// the absolute value of the rotation angle
	float yAnglesAbs = Mathf.Abs(yAngles);
	// how much we should rotate each frame
	float rotationPerFrame = yAngles / (yAnglesAbs * inTime);
	//float rotationPerFrame = yAngles >= 0 ? 1 / inTime : -1 / inTime;
	while(rotatedTotal < yAnglesAbs && rotatedTotal > -yAnglesAbs) {
		rotatedTotal += rotationPerFrame;
		// if we are about to rotate it too far
		if (rotatedTotal > yAnglesAbs || rotatedTotal < -yAnglesAbs) {
			// only rotate the menu as much as there is left to rotate
			transform.Rotate(0, rotationPerFrame - (rotatedTotal - yAngles), 0);

		} else {
			transform.Rotate(0, rotationPerFrame, 0);
		}
		yield return null;
	}
	// fromAngle = transform.rotation.eulerAngles;
	isRotating = false;
	if(active) {
		backsideRenderer.enabled = false;
		buttonCollider.enabled = true;
		frontsideRenderer.sprite = standardTexture;
	} else {
		frontsideRenderer.enabled = false;
	}
}

}
