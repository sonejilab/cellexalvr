using UnityEngine;
using System.Collections;


namespace CellexalVR.Tutorial
{

    public class BringBackObj : MonoBehaviour
    {
        private Vector3 defaultPosition;
        private Quaternion defaultRotation;
        // Use this for initialization
        void Start()
        {
            SavePosition();
        }

        // Update is called once per frame
        void Update()
        {

        }

        internal void ResetPosition()
        {
            transform.localPosition = defaultPosition;
            transform.localRotation = defaultRotation;
        }

        internal void SavePosition()
        {
            defaultPosition = transform.localPosition;
            defaultRotation = transform.localRotation;
        }
    }
}
