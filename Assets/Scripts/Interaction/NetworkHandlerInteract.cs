using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Handles what happens when a network handler is interacted with.
    /// </summary>
    class NetworkHandlerInteract : InteractableObjectBasic
    {
        public ReferenceManager referenceManager;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

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
            referenceManager.multiuserMessageSender.SendMessageDisableColliders(gameObject.name);
            GetComponent<NetworkHandler>().ToggleNetworkColliders(false);
        }

        private void UnGrabbed(object sender, Hand hand)
        {
            base.OnInteractableObjectUnGrabbed(hand);
            referenceManager.multiuserMessageSender.SendMessageEnableColliders(gameObject.name);
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            referenceManager.multiuserMessageSender.SendMessageNetworkUngrabbed(gameObject.name, transform.position, transform.rotation, rigidbody.velocity, rigidbody.angularVelocity);
            GetComponent<NetworkHandler>().ToggleNetworkColliders(true);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.TwoLasers
                || referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.Keyboard)
            {
                if (other.CompareTag("Controller"))
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
                if (other.CompareTag("Controller"))
                {
                    CellexalEvents.ObjectUngrabbed.Invoke();
                }
            }
        }
    }
}