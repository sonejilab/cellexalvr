using UnityEngine;
using System.Collections;

public class RotateMenu : MonoBehaviour {

private float finalZRotation = 0;
private bool isRotating = false;
private Vector3 fromAngle;
public void RotateRight() {
	if (!isRotating) {
		isRotating = true;
		StartCoroutine(RotateMe(-90f, 0.15f));
	}
}

public void RotateLeft() {
	if (!isRotating) {
		isRotating = true;
		StartCoroutine(RotateMe(90f, 0.15f));
	}
}

IEnumerator RotateMe(float zAngles, float inTime) {
	// how much we have rotated so far
	float rotatedAngles = 0;
	// how much we should rotate each frame
	float rotated = zAngles / (90 * inTime);
	while(rotatedAngles < 90f && rotatedAngles > -90f) {
		rotatedAngles += rotated;
		// if we are about to rotate it too far
		if (rotatedAngles > 90f || rotatedAngles < -90f) {
			// only rotate the menu as much as there is left to rotate
			transform.Rotate(0, 0, rotatedAngles - zAngles);
		} else {
			transform.Rotate(0, 0, rotated);
		}
		yield return null;
	}
	// fromAngle = transform.rotation.eulerAngles;
	isRotating = false;
}

}
