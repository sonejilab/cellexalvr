namespace VRTK.GrabAttachMechanics
{

    using UnityEngine;

    public class HeatmapGrab : VRTK_InteractableObject
    {
        public ReferenceManager referenceManager;

        public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
        {
            print("GRABBING NETWORK");
            referenceManager.gameManager.InformDisableColliders(gameObject.name);
            GetComponent<MeshCollider>().convex = true;
            base.OnInteractableObjectGrabbed(e);
        }

        public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
        {
            print("UNGRABBING NETWORK");
            referenceManager.gameManager.InformEnableColliders(gameObject.name);
            if (grabbingObjects.Count == 0)
                GetComponent<MeshCollider>().convex = false;
            base.OnInteractableObjectUngrabbed(e);
        }

    }

}
