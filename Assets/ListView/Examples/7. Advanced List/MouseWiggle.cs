using UnityEngine;
using System.Collections;

public class MouseWiggle : MonoBehaviour
{
	public float speed = -0.1f;
	public Transform pivot;
	void Update () {
		transform.RotateAround(pivot.position, Vector3.up, Input.GetAxis("Mouse X") * Time.deltaTime * speed);
	}
}
