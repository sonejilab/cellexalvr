using CellexalVR.General;
using UnityEngine;

namespace CellexalVR.AnalysisObjects
{

    public class ReattachLegendButton : MonoBehaviour
    {
        public ReferenceManager referenceManager;

        private bool controllerInside = false;
        private SteamVR_TrackedObject rightController;
        private SteamVR_Controller.Device device;
        private MeshRenderer meshRenderer;

        private void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            rightController = referenceManager.rightController;
            meshRenderer = GetComponent<MeshRenderer>();
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

        private void Update()
        {
            var device = SteamVR_Controller.Input((int)rightController.index);
            if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                LegendManager parentLegendManager = gameObject.GetComponentInParent<LegendManager>();
                parentLegendManager.transform.parent = parentLegendManager.attachPoint.transform;
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
