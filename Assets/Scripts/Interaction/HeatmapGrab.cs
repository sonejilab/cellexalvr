using CellexalVR.General;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Controls the grabbing of the heatmap.
    /// </summary>
    public class HeatmapGrab : InteractableObjectBasic
    {
        public ReferenceManager referenceManager;

        protected override void Awake()
        {
            base.Awake();
            InteractableObjectGrabbed += Grabbed;
            InteractableObjectUnGrabbed += UnGrabbed;
        }

        private void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }

        private void Grabbed(object sender, Hand hand)
        {
            base.OnInteractableObjectGrabbed(hand);
            referenceManager.multiuserMessageSender.SendMessageToggleGrabbable(gameObject.name, false);
            GetComponent<MeshCollider>().convex = true;
        }

        private void UnGrabbed(object sender, Hand hand)
        {
            base.OnInteractableObjectUnGrabbed(hand);
            referenceManager.multiuserMessageSender.SendMessageToggleGrabbable(gameObject.name, true);
            GetComponent<MeshCollider>().convex = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.TwoLasers
                || referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.Keyboard)
            {
                if (other.CompareTag("Player"))
                {
                    CellexalEvents.ObjectGrabbed.Invoke();
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.TwoLasers
                || referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.Keyboard)
            {
                if (other.CompareTag("Player"))
                {
                    CellexalEvents.ObjectUngrabbed.Invoke();
                }
            }
        }
    }
}