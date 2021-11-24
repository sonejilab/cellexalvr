using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Handles what happens when a network handler is interacted with.
    /// </summary>
    class NetworkHandlerInteract : OffsetGrab
    {
        public ReferenceManager referenceManager;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            CellexalEvents.NetworkCreated.AddListener(RegisterColliders);
        }

        private void RegisterColliders()
        {
            print("register colliders1");
            enabled = false;
            colliders.AddRange(gameObject.GetComponents<BoxCollider>());
            enabled = true;
        }
        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            referenceManager.multiuserMessageSender.SendMessageDisableColliders(gameObject.name);
            GetComponent<NetworkHandler>().ToggleNetworkColliders(false);
            base.OnSelectEntering(args);
        }

        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            referenceManager.multiuserMessageSender.SendMessageEnableColliders(gameObject.name);
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            referenceManager.multiuserMessageSender.SendMessageNetworkUngrabbed(gameObject.name, transform.position, transform.rotation, rigidbody.velocity, rigidbody.angularVelocity);
            GetComponent<NetworkHandler>().ToggleNetworkColliders(true);
            base.OnSelectExiting(args);
        }

        //public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
        //{
        //    referenceManager.multiuserMessageSender.SendMessageDisableColliders(gameObject.name);
        //    // moving many triggers really pushes what unity is capable of
        //    //foreach (Collider c in GetComponentsInChildren<Collider>())
        //    //{
        //    //    if (c.gameObject.name == "Ring")
        //    //    {
        //    //        ((MeshCollider)c).convex = true;
        //    //    }
        //    //}
        //    GetComponent<NetworkHandler>().ToggleNetworkColliders(false);
        //    base.OnInteractableObjectGrabbed(e);
        //}

        //public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
        //{
        //    referenceManager.multiuserMessageSender.SendMessageEnableColliders(gameObject.name);
        //    Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        //    referenceManager.multiuserMessageSender.SendMessageNetworkUngrabbed(gameObject.name, transform.position, transform.rotation, rigidbody.velocity, rigidbody.angularVelocity);
        //    //foreach (Collider c in GetComponentsInChildren<Collider>())
        //    //{
        //    //    if (c.gameObject.name == "Ring")
        //    //    {
        //    //        ((MeshCollider)c).convex = false;
        //    //    }
        //    //}
        //    GetComponent<NetworkHandler>().ToggleNetworkColliders(true);
        //    base.OnInteractableObjectUngrabbed(e);
        //}

        //private void OnTriggerEnter(Collider other)
        //{
        //    if (referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.TwoLasers
        //        || referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.Keyboard)
        //    {
        //        if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
        //        {
        //            CellexalEvents.ObjectGrabbed.Invoke();
        //        }
        //    }
        //}

        //private void OnTriggerExit(Collider other)
        //{
        //    if (referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.TwoLasers
        //        || referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.Keyboard)
        //    {
        //        if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
        //        {
        //            CellexalEvents.ObjectUngrabbed.Invoke();
        //        }
        //    }
        //}
    }
}