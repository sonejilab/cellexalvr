using UnityEngine;
using System.Collections;
namespace CurvedVRKeyboard {

    public class MouseCam: MonoBehaviour {

        public float speedH = 2.5f;
        public float speedV = 2.5f;

        private float yaw = 0.0f;
        private float pitch = 0.0f;

        // Use this for initialization
        void Start () {
            yaw = transform.rotation.eulerAngles.y;
        }

        void Update () {
            yaw += speedH * Input.GetAxis("Mouse X");
            pitch -= speedV * Input.GetAxis("Mouse Y");

            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);
        }

    }
}