using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Handles what happens when a graph is interacted with.
    /// </summary>
    class GraphInteract : XRGrabInteractable
    {
        public ReferenceManager referenceManager;

        //private Coroutine runningCoroutine;

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

        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            base.OnSelectEntering(args);
            referenceManager.multiuserMessageSender.SendMessageToggleGrabbable(gameObject.name, false);
        }

        //public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
        //{
        //    referenceManager.multiuserMessageSender.SendMessageToggleGrabbable(gameObject.name, false);
        //    //StopPositionSync();
        //    base.OnInteractableObjectGrabbed(e);
        //}


        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            referenceManager.multiuserMessageSender.SendMessageToggleGrabbable(gameObject.name, true);
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            referenceManager.multiuserMessageSender.SendMessageGraphUngrabbed(gameObject.name, transform.position, transform.rotation, rigidbody.velocity, rigidbody.angularVelocity);
            base.OnSelectExiting(args);
        }

        //public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
        //{
        //    referenceManager.multiuserMessageSender.SendMessageToggleGrabbable(gameObject.name, true);
        //    Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
        //    referenceManager.multiuserMessageSender.SendMessageGraphUngrabbed(gameObject.name, transform.position, transform.rotation, rigidbody.velocity, rigidbody.angularVelocity);
        //    //referenceManager.rightLaser.enabled = true;
        //    //referenceManager.controllerModelSwitcher.ActivateDesiredTool();
        //    //runningCoroutine = StartCoroutine(KeepGraphPositionSynched());
        //    base.OnInteractableObjectUngrabbed(e);
        //}

    }
}