using CellexalVR.General;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Controls the grabbing of the heatmap.
    /// </summary>
    public class HeatmapGrab : XRGrabInteractable
    {
        public ReferenceManager referenceManager;

        private void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }


        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            referenceManager.multiuserMessageSender.SendMessageToggleGrabbable(gameObject.name, false);
            GetComponent<MeshCollider>().convex = true;
            base.OnSelectEntering(args);
        }

        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            referenceManager.multiuserMessageSender.SendMessageToggleGrabbable(gameObject.name, true);
            GetComponent<MeshCollider>().convex = false;
            base.OnSelectExiting(args);
        }

        //public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
        //{
        //    referenceManager.multiuserMessageSender.SendMessageToggleGrabbable(gameObject.name, false);
        //    GetComponent<MeshCollider>().convex = true;
        //    base.OnInteractableObjectGrabbed(e);
        //}

        //public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
        //{
        //    referenceManager.multiuserMessageSender.SendMessageToggleGrabbable(gameObject.name, true);
        //    if (grabbingObjects.Count == 0)
        //        GetComponent<MeshCollider>().convex = false;
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