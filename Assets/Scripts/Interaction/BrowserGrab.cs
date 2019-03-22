using CellexalVR.General;
using UnityEngine;
using VRTK;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Interaction with Web browser. Keyboard is set inactive when grabbing for more reliable moving of the key-panels.
    /// </summary>
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
            //GetComponent<MeshCollider>().convex = true;
            base.OnInteractableObjectGrabbed(e);
        }

        public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
        {
            keyboard.SetActive(true);
            referenceManager.gameManager.InformEnableColliders(gameObject.name);
            //if (grabbingObjects.Count == 0)
            //    GetComponent<MeshCollider>().convex = false;
            base.OnInteractableObjectUngrabbed(e);
        }

    }

}
