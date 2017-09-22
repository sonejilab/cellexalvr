using UnityEngine;

/// <summary>
/// This class rotates the text on the arcs between networks and positions the text on the middle of the arcs.
/// </summary>
public class HelperTextRotator : MonoBehaviour
{
    //[Tooltip("Set these to 0 if the text should not rotate around that axis, or 1 if they should")]
    //public Vector3 axisToRotate;
    private Transform CameraToLookAt;

    void Start()
    {
        CameraToLookAt = GameObject.Find("Camera (eye)").transform;
    }

    void Update()
    {
        // some math make the text not be mirrored
        transform.LookAt(2 * transform.position - CameraToLookAt.position);

    }

}
