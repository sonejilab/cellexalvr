namespace VRTK.GrabAttachMechanics
{

    using UnityEngine;

    public class BrowserGrab : VRTK_InteractableObject
    {
        public ReferenceManager referenceManager;
        public GameObject keyboard;
        

        public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
        {   
            keyboard.SetActive(false);
            if (referenceManager == null)
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
            referenceManager.gameManager.InformDisableColliders(gameObject.name);
            GetComponent<MeshCollider>().convex = true;
            base.OnInteractableObjectGrabbed(e);
        }

        public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
        {
            keyboard.SetActive(true);
            referenceManager.gameManager.InformEnableColliders(gameObject.name);
            if (grabbingObjects.Count == 0)
                GetComponent<MeshCollider>().convex = false;
            base.OnInteractableObjectUngrabbed(e);
        }

    }

}
