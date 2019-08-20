using UnityEngine;
using System.Collections;

namespace CellexalVR.SceneObjects
{

    public class LineRendererFollowTransforms : MonoBehaviour
    {

        public LineRenderer lineRenderer;
        public Transform transform1;
        public Transform transform2;




        private void Update()
        {
            if (transform1.hasChanged || transform2.hasChanged)
            {
                lineRenderer.SetPositions(new Vector3[]
                {
                    transform1.position,
                    transform2.position
                });
            }


        }
    }
}
