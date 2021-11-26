using UnityEngine;


namespace CellexalVR.SceneObjects
{

    /// <summary>
    /// This class rotates the text on the arcs between networks and positions the text on the middle of the arcs.
    /// </summary>
    public class TextRotator : MonoBehaviour
    {

        private Transform CameraToLookAt;
        private Transform t1, t2, t3, t4;

        void Start()
        {
            CameraToLookAt = GameObject.Find("Main Camera").transform;
        }

        void Update()
        {
            // some math make the text not be mirrored
            transform.LookAt(2 * transform.position - CameraToLookAt.position);

            // The text should be in the middle of four transforms
            if (t1 != null && t2 != null && t3 != null && t4 != null)
            {
                Vector3 midPoint1 = (t1.position + t2.position) / 2;
                Vector3 midPoint2 = (t3.position + t4.position) / 2;
                transform.position = (midPoint1 + midPoint2) / 2;
            }

        }

        /// <summary>
        /// Sets the transforms that this text should be in the middle of.
        /// </summary>
        /// <param name="t1"> The first transform. </param>
        /// <param name="t2"> The second transform. </param>
        /// <param name="t3"> The third transform. </param>
        /// <param name="t4"> The fourth transform. </param>
        public void SetTransforms(Transform t1, Transform t2, Transform t3, Transform t4)
        {
            this.t1 = t1;
            this.t2 = t2;
            this.t3 = t3;
            this.t4 = t4;
        }
    }
}