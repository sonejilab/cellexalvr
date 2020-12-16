using CellexalVR.General;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace CellexalVR.AnalysisObjects
{

    public class ReattachLegendButton : MonoBehaviour
    {
        private bool controllerInside = false;
        private SteamVR_Behaviour_Pose rightController;
        private MeshRenderer meshRenderer;

        private void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                SetHighlighted(true);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                SetHighlighted(false);
            }
        }

        private void Update()
        {
            if (controllerInside && Player.instance.rightHand.grabPinchAction.GetStateDown(Player.instance.rightHand.handType))
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
