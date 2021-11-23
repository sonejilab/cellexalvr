using CellexalVR.General;
using CellexalVR.Interaction;
using UnityEngine;

namespace CellexalVR.AnalysisObjects
{

    public class ReattachLegendButton : MonoBehaviour
    {
        public ReferenceManager referenceManager;

        private bool controllerInside = false;
        // Open XR 
		//private SteamVR_Controller.Device device;
		private UnityEngine.XR.Interaction.Toolkit.ActionBasedController rightController;
        // Open XR 
		//private SteamVR_Controller.Device device;
		private UnityEngine.XR.InputDevice device;
        private MeshRenderer meshRenderer;

        private void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            rightController = referenceManager.rightController;
            meshRenderer = GetComponent<MeshRenderer>();
            CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);
        }

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Controller"))
            {
                SetHighlighted(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Controller"))
            {
                SetHighlighted(false);
            }
        }

        private void OnTriggerClick()
        {
            // Open XR
            //device = SteamVR_Controller.Input((int)rightController.index);
            if (controllerInside)
            {
                LegendManager parentLegendManager = gameObject.GetComponentInParent<LegendManager>();
                //parentLegendManager.transform.parent = parentLegendManager.attachPoint.transform;
                parentLegendManager.transform.localPosition = Vector3.zero;
                parentLegendManager.transform.localRotation = Quaternion.identity;
                Destroy(parentLegendManager.GetComponent<Rigidbody>());
                SetHighlighted(false);
            }

        }

        public void SetHighlighted(bool highlight)
        {
            controllerInside = highlight;
            meshRenderer.material.mainTextureOffset = new Vector2(highlight ? 0.5f : 0, 0);
        }
    }
}
