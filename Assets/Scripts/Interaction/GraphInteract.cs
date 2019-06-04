using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using System.Collections;
using UnityEngine;
using VRTK;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Handles what happens when a graph is interacted with.
    /// </summary>
    class GraphInteract : VRTK_InteractableObject
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

        public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
        {
            referenceManager.gameManager.InformToggleGrabbable(gameObject.name, false);
            //StopPositionSync();
            //referenceManager.controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Normal);
            //referenceManager.gameManager.InformMoveGraph(GetComponent<Graph>().GraphName, transform.position, transform.rotation, transform.localScale);
            base.OnInteractableObjectGrabbed(e);
        }

        public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
        {
            referenceManager.gameManager.InformToggleGrabbable(gameObject.name, true);
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            referenceManager.gameManager.InformGraphUngrabbed(gameObject.name, rigidbody.velocity, rigidbody.angularVelocity);
            //referenceManager.rightLaser.enabled = true;
            //referenceManager.controllerModelSwitcher.ActivateDesiredTool();
            //runningCoroutine = StartCoroutine(KeepGraphPositionSynched());
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