using CellexalVR.General;
using UnityEngine;
using Valve.VR.InteractionSystem;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Interaction with Web browser. Keyboard is set inactive when grabbing for more reliable moving of the key-panels.
    /// </summary>
    public class BrowserGrab : InteractableObjectBasic 
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

        protected override void Awake()
        {
            base.Awake();
            InteractableObjectGrabbed += Grabbed;
            InteractableObjectUnGrabbed += UnGrabbed;
        }


        private void Grabbed(object sender, Hand hand)
        {
            base.OnInteractableObjectGrabbed(hand);
            keyboard.SetActive(false);
            if (referenceManager == null)
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
            referenceManager.multiuserMessageSender.SendMessageDisableColliders(gameObject.name);
            //GetComponent<MeshCollider>().convex = true;
        }

        private void UnGrabbed(object sender, Hand hand)
        {
            base.OnInteractableObjectUnGrabbed(hand);
            keyboard.SetActive(true);
            referenceManager.multiuserMessageSender.SendMessageEnableColliders(gameObject.name);
            //if (grabbingObjects.Count == 0)
            //    GetComponent<MeshCollider>().convex = false;
        }

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
