using UnityEngine;

namespace CellexalVR.SceneObjects
{

    /// <summary>
    /// Rotates the text on the arcs between networks and positions the text on the middle of the arcs.
    /// </summary>
    public class SimpleTextRotator : MonoBehaviour
    {

        private Transform CameraToLookAt;
        private Transform t1, t2;

        void Start()
        {
            CameraToLookAt = GameObject.Find("Camera (eye)").transform;
        }

        void Update()
        {
            // some math make the text not be mirrored
            if (t1 != null && t2 != null)
            {
                transform.LookAt(2 * transform.position - CameraToLookAt.position);
                transform.position = (t1.position + t2.position) / 2;
            }

        }

        public void SetTransforms(Transform t1, Transform t2)
        {
            this.t1 = t1;
            this.t2 = t2;
        }
    }
}