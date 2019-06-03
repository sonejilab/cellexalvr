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

        private Coroutine runningCoroutine;

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
            if (runningCoroutine != null)
            {
                StopCoroutine(runningCoroutine);
            }
            //referenceManager.controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Normal);
            //referenceManager.gameManager.InformMoveGraph(GetComponent<Graph>().GraphName, transform.position, transform.rotation, transform.localScale);
            base.OnInteractableObjectGrabbed(e);
        }

        public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
        {
            referenceManager.gameManager.InformToggleGrabbable(gameObject.name, true);
            //referenceManager.rightLaser.enabled = true;
            //referenceManager.controllerModelSwitcher.ActivateDesiredTool();
            runningCoroutine = StartCoroutine(KeepGraphPositionSynched(3f));
            base.OnInteractableObjectUngrabbed(e);
        }

        private IEnumerator KeepGraphPositionSynched(float time)
        {
            if (!referenceManager.gameManager.multiplayer)
            {
                yield break;
            }
            string graphName = gameObject.GetComponent<Graph>().GraphName;
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            while (time > 0f && rigidbody.velocity.magnitude > 0.001f)
            {
                referenceManager.gameManager.InformMoveGraph(graphName, transform.position, transform.rotation, transform.localScale);
                time -= Time.deltaTime;
                yield return null;
            }
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