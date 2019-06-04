using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using System.Collections;
using UnityEngine;
using VRTK;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Handles what happens when a network handler is interacted with.
    /// </summary>
    class NetworkHandlerInteract : VRTK_InteractableObject
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
        }

        public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
        {
            referenceManager.gameManager.InformDisableColliders(gameObject.name);
            // moving many triggers really pushes what unity is capable of
            //foreach (Collider c in GetComponentsInChildren<Collider>())
            //{
            //    if (c.gameObject.name == "Ring")
            //    {
            //        ((MeshCollider)c).convex = true;
            //    }
            //}
            GetComponent<NetworkHandler>().ToggleNetworkColliders(false);
            base.OnInteractableObjectGrabbed(e);
        }

        public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
        {
            referenceManager.gameManager.InformEnableColliders(gameObject.name);
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            referenceManager.gameManager.InformNetworkUngrabbed(gameObject.name, rigidbody.velocity, rigidbody.angularVelocity);
            //foreach (Collider c in GetComponentsInChildren<Collider>())
            //{
            //    if (c.gameObject.name == "Ring")
            //    {
            //        ((MeshCollider)c).convex = false;
            //    }
            //}
            GetComponent<NetworkHandler>().ToggleNetworkColliders(true);
            base.OnInteractableObjectUngrabbed(e);
        }

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