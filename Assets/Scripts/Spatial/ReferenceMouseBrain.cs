using UnityEngine;
using System.Collections;
using CellexalVR.General;
using CellexalVR.Spatial;

namespace CellexalVR.Spatial
{

    public class ReferenceMouseBrain : MonoBehaviour
    {
        public GameObject brainModel;
        public SpatialGraph spatialGraph;
        public ReferenceManager referenceManager;

        private bool attached;
        private bool controllerInside;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void AttachToGraph()
        {
            if (!attached)
            {
                Destroy(brainModel.GetComponent<Collider>());
                Destroy(brainModel.GetComponent<Rigidbody>());
                transform.parent = spatialGraph.transform;
            }
            else
            {
                transform.parent = null;
                brainModel.AddComponent<BoxCollider>();
                var rigidbody = brainModel.GetComponent<Rigidbody>();
                if (rigidbody == null)
                {
                    rigidbody = brainModel.AddComponent<Rigidbody>();
                }
                rigidbody.useGravity = false;
                rigidbody.isKinematic = false;
                rigidbody.drag = 10;
                rigidbody.angularDrag = 15;
            }
            attached = !attached;

        }
    }
}
