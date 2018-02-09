namespace VRTK.GrabAttachMechanics
{

    using UnityEngine;

    public class HeatmapGrab : VRTK_InteractableObject
    {

        public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
        {
            GetComponent<MeshCollider>().convex = true;
            base.OnInteractableObjectGrabbed(e);
        }

        public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
        {
            if (grabbingObjects.Count == 2)
                GetComponent<MeshCollider>().convex = false;
            base.OnInteractableObjectUngrabbed(e);
        }

    }

}
