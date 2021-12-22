using System.Windows.Input;
using System.Collections;
using UnityEngine;

namespace CellexalVR.Multiuser
{
    /// <summary>
    /// Handles camera viewing with mouse if the user is not using a VR headset.
    /// </summary>
    public class SpectatorCameraLook : MonoBehaviour
    {

        public float smoothing;
        public float sensitivity;
        private Vector2 mouseLook;
        private Vector2 smoothingV;
        private Transform controller;

        void Start()
        {
            controller = GetComponentInParent<SpectatorController>().transform;
        }

        private void Update()
        {
            if (Input.GetMouseButton(1))
            {
                var dir = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
                smoothingV.x = Mathf.Lerp(smoothingV.x, dir.x, 1f / smoothing);
                smoothingV.y = Mathf.Lerp(smoothingV.y, dir.y, 1f / smoothing);
                mouseLook += smoothingV;


                transform.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
                controller.localRotation = Quaternion.AngleAxis(mouseLook.x, controller.transform.up);
            }
        }
    }
}