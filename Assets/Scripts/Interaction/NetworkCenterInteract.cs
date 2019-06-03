using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using System.Collections;
using UnityEngine;
using VRTK;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Handles what happens when a network center is interacted with.
    /// </summary>
    class NetworkCenterInteract : VRTK_InteractableObject
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
            referenceManager.gameManager.InformDisableColliders(gameObject.name);
            if (grabbingObjects.Count == 1)
            {
                // moving many triggers really pushes what unity is capable of
                foreach (Collider c in GetComponentsInChildren<Collider>())
                {
                    if (c.gameObject.name != "Ring" && !c.gameObject.name.Contains("Enlarged_Network"))
                    {
                        c.enabled = false;
                    }
                    //else if (c.gameObject.name == "Ring")
                    //{
                    //    ((MeshCollider)c).convex = true;
                    //}
                }
            }
            if (runningCoroutine != null)
            {
                StopCoroutine(runningCoroutine);
            }
            base.OnInteractableObjectGrabbed(e);
        }

        public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
        {
            referenceManager.gameManager.InformEnableColliders(gameObject.name);
            if (grabbingObjects.Count == 0)
            {
                foreach (Collider c in GetComponentsInChildren<Collider>())
                {
                    if (c.gameObject.name != "Ring" && !c.gameObject.name.Contains("Enlarged_Network"))
                    {
                        c.enabled = true;
                    }
                    //else if (c.gameObject.name == "Ring")
                    //{
                    //    ((MeshCollider)c).convex = false;
                    //}

                }
            }
            runningCoroutine = StartCoroutine(KeepPositionSynched(3f));
            base.OnInteractableObjectUngrabbed(e);
        }


        private IEnumerator KeepPositionSynched(float time)
        {
            if (!referenceManager.gameManager.multiplayer)
            {
                yield break;
            }
            NetworkCenter center = gameObject.GetComponent<NetworkCenter>();
            string networkCenterName = center.name;
            string networkHandlerName = center.Handler.name;
            Rigidbody rigidbody = gameObject.GetComponent<Rigidbody>();
            while (time > 0f || rigidbody.velocity.magnitude > 0.001f)
            {
                referenceManager.gameManager.InformMoveNetworkCenter(networkHandlerName, networkCenterName, transform.position, transform.rotation, transform.localScale);
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