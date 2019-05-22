using CellexalVR.General;
using UnityEngine;
using VRTK;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Controls the grabbing of the heatmap.
    /// </summary>
    public class HeatmapGrab : VRTK_InteractableObject
    {
        public ReferenceManager referenceManager;

        private void Start()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }

        public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
        {

            referenceManager.gameManager.InformDisableColliders(gameObject.name);
            GetComponent<MeshCollider>().convex = true;
            base.OnInteractableObjectGrabbed(e);
        }

        public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
        {
            referenceManager.gameManager.InformEnableColliders(gameObject.name);
            if (grabbingObjects.Count == 0)
                GetComponent<MeshCollider>().convex = false;
            base.OnInteractableObjectUngrabbed(e);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.Equals("Collider"))
            {
                CellexalEvents.ObjectGrabbed.Invoke();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name.Equals("Collider"))
            {
                CellexalEvents.ObjectUngrabbed.Invoke();
            }
        }

    }

}