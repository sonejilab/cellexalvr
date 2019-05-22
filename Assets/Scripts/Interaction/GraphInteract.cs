using CellexalVR.General;
using UnityEngine;
using VRTK;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Handles what happens when a graph is interacted with.
    /// </summary>
    class GraphInteract : VRTK_InteractableObject
    {
        public ReferenceManager referenceManager;

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
            referenceManager.gameManager.InformToggleGrabbable(gameObject.name, false);
            //referenceManager.controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Normal);
            //referenceManager.gameManager.InformMoveGraph(GetComponent<Graph>().GraphName, transform.position, transform.rotation, transform.localScale);
            base.OnInteractableObjectGrabbed(e);
        }

        public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
        {
            referenceManager.gameManager.InformToggleGrabbable(gameObject.name, true);
            //referenceManager.rightLaser.enabled = true;
            //referenceManager.controllerModelSwitcher.ActivateDesiredTool();
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