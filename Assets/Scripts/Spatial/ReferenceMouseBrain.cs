using UnityEngine;
using System.Collections;
using CellexalVR.General;
using CellexalVR.Spatial;
using CellexalVR.Interaction;

namespace CellexalVR.Spatial
{

    public class ReferenceMouseBrain : MonoBehaviour
    {
        public GameObject brainModel;
        public SpatialGraph spatialGraph;
        public ReferenceManager referenceManager;

        private bool attached;
        // Open XR 
		//private SteamVR_Controller.Device device;
		private UnityEngine.XR.Interaction.Toolkit.ActionBasedController rightController;
        // Open XR 
		//private SteamVR_Controller.Device device;
		private UnityEngine.XR.InputDevice device;
        private bool controllerInside;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            CellexalEvents.GraphsUnloaded.AddListener(() => Destroy(this.gameObject));
            if (!referenceManager)
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
            rightController = referenceManager.rightController;
            CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
            {
                controllerInside = true;
                GetComponent<Renderer>().material.color = Color.red;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
            {
                controllerInside = false;
                GetComponent<Renderer>().material.color = Color.white;
            }
        }

        private void OnTriggerClick()
        {
            // Open XR
            //device = SteamVR_Controller.Input((int)rightController.index);
            if (controllerInside)
            {
                AttachToGraph();
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
