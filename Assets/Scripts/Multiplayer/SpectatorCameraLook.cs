using System.Windows.Input;
using System.Collections;
using UnityEngine;

namespace CellexalVR.Multiplayer
{

    /// <summary>
    /// Handles camera viewing with mouse if the user is not using a VR headset.
    /// </summary>
    public class SpectatorCameraLook : MonoBehaviour
    {
        Vector2 mouseLook;
        Vector2 smoothingV;

        public float smoothing;
        public float sensitivity;

        GameObject controller;
        void Start()
        {
            controller = transform.parent.gameObject;
        }

        private void Update()
        {
            var dir = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));
            smoothingV.x = Mathf.Lerp(smoothingV.x, dir.x, 1f / smoothing);
            smoothingV.y = Mathf.Lerp(smoothingV.y, dir.y, 1f / smoothing);
            mouseLook += smoothingV;


            transform.localRotation = Quaternion.AngleAxis(-mouseLook.y, Vector3.right);
            controller.transform.localRotation = Quaternion.AngleAxis(mouseLook.x, controller.transform.up);
        }

    }
}

