using CellexalVR.General;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Interaction with Web browser. Keyboard is set inactive when grabbing for more reliable moving of the key-panels.
    /// </summary>
    public class BrowserGrab : XRGrabInteractable
    {
        public ReferenceManager referenceManager;
        public GameObject keyboard;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            keyboard.SetActive(false);
            if (referenceManager == null)
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
            referenceManager.multiuserMessageSender.SendMessageDisableColliders(gameObject.name);
            base.OnSelectEntering(args);
        }

        //public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
        //{
        //    keyboard.SetActive(false);
        //    if (referenceManager == null)
        //    {
        //        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        //    }
        //    referenceManager.multiuserMessageSender.SendMessageDisableColliders(gameObject.name);
        //    //GetComponent<MeshCollider>().convex = true;
        //    base.OnInteractableObjectGrabbed(e);
        //}

        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            keyboard.SetActive(true);
            referenceManager.multiuserMessageSender.SendMessageEnableColliders(gameObject.name);
            base.OnSelectExiting(args);
        }

        //public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
        //{
        //    keyboard.SetActive(true);
        //    referenceManager.multiuserMessageSender.SendMessageEnableColliders(gameObject.name);
        //    //if (grabbingObjects.Count == 0)
        //    //    GetComponent<MeshCollider>().convex = false;
        //    base.OnInteractableObjectUngrabbed(e);
        //}

        //private void OnTriggerEnter(Collider other)
        //{
        //    if (referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.TwoLasers)
        //    {
        //        if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
        //        {
        //            CellexalEvents.ObjectGrabbed.Invoke();
        //        }
        //    }
        //}

        //private void OnTriggerExit(Collider other)
        //{
        //    if (referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.TwoLasers)
        //    {
        //        if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
        //        {
        //            CellexalEvents.ObjectUngrabbed.Invoke();
        //        }
        //    }
        //}

    }

}
