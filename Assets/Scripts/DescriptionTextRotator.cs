using UnityEngine;

public class DescriptionTextRotator : MonoBehaviour {
public GameObject cameraToFollow;
// private Vector3 pivot;
void Start() {
	//pivot = new Vector3(0f, 0f, 0.8f);
}

void Update() {
	// pivot = transform.parent.position + new Vector3(0f, 0f, 0.8f);
	// transform.position = Vector3.Slerp((transform.position - pivot), (cameraToFollow.transform.position - pivot), 0.1f * Time.deltaTime);
	// transform.position += pivot;
	// transform.LookAt(cameraToFollow.transform);
	// transform.Rotate(0f, 180f, 0f);
	// // set x rotation to 0, don't change the others
	// transform.rotation = Quaternion.Euler(transform.parent.parent.eulerAngles.x, 0, -transform.parent.parent.eulerAngles.y);
	//
	// print(transform.parent.parent.name);

	//	transform.rotation = transform.parent.parent.transform.rotation;
	//transform.rotation.eulerAngles.Scale(new Vector3(1f, 0, 1f));

	// print("parentRotationX: " + transform.parent.parent.eulerAngles.x);
	// print("rotationX: " + transform.eulerAngles.x);
	// print("parentRotationY: " + transform.parent.parent.eulerAngles.y);
	// print("rotationY: " + transform.eulerAngles.y);
	// print("parentRotationZ: " + transform.parent.parent.eulerAngles.z);
	// print("rotationZ: " + transform.eulerAngles.z);

	// = transform.parent.eulerAngles.x;
	//transform.rotation.eulerAngles.y = transform.parent.eulerAngles.y;

	// print("rotationX: " + transform.localEulerAngles.x);
	// print("rotationY: " + transform.localEulerAngles.y);
	// print("rotationZ: " + transform.localEulerAngles.z);
}

}
