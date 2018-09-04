namespace VRTK.GrabAttachMechanics
{

    using UnityEngine;

    public class HeatmapGrab : VRTK_InteractableObject
    {
        public ReferenceManager referenceManager;

        public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
        {
            GetComponent<MeshCollider>().convex = true;
            base.OnInteractableObjectGrabbed(e);
            referenceManager.gameManager.InformDisableColliders(gameObject.name);
        }

        public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
        {
            if (grabbingObjects.Count == 0)
                GetComponent<MeshCollider>().convex = false;
            base.OnInteractableObjectUngrabbed(e);
            referenceManager.gameManager.InformEnableColliders(gameObject.name);
        }

    }

}
