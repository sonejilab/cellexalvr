using UnityEngine;

public class DescriptionTextSphereRotator : MonoBehaviour {
public GameObject cameraToFollow;
// private Vector3 pivot;
void Start() {
	//pivot = new Vector3(0f, 0f, 0.8f);
}

void Update() {
	// pivot = transform.parent.position + new Vector3(0f, 0f, 0.8f);
	// transform.position = Vector3.Slerp((transform.position - pivot), (cameraToFollow.transform.position - pivot), 0.1f * Time.deltaTime);
	// transform.position += pivot;ff
	/*Vector3 lookPos = cameraToFollow.transform.position - transform.position;
	   lookPos.x = 0;
	   lookPos.y = 0;
	   Quaternion rotation = Quaternion.LookRotation(lookPos);
	   transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 10f * Time.deltaTime);*/
	Vector3 targetPosition = new Vector3(cameraToFollow.transform.position.x, cameraToFollow.transform.position.y, transform.position.z);
	transform.LookAt(targetPosition);
	// transform.LookAt(cameraToFollow.transform, Vector3.right);
	//transform.localRotation.SetLookRotation(transform.rotation.eulerAngles, Vector3.right);

	// transform.Rotate(90f, 0f, 0f);
	//transform.rotation = Quaternion.Euler(transform.parent.parent.eulerAngles.x, transform.parent.parent.eulerAngles.y, transform.rotation.eulerAngles.z);
	// set x rotation to 0, don't change the others
	// transform.rotation.eulerAngles.Scale(new Vector3(0f, 0f, 1f));
}

}
