using UnityEngine;

namespace CellexalVR.General
{

    /// <summary>
    /// Rotates a gameobject so it faces the camera.
    /// </summary>
    public class LookAtCamera : MonoBehaviour
    {
        private Transform CameraToLookAt;

        void Start()
        {
            CameraToLookAt = GameObject.Find("Camera (eye)").transform;
        }

        void Update()
        {
            transform.LookAt(CameraToLookAt);
            transform.Rotate(0f, 180f, 0f);
        }
    }
}
