using UnityEngine;
using VRTK;

namespace CellexalVR.Interaction
{
    public class DetachFromParentOnGrab : VRTK_InteractableObject
    {

        private bool unsetParent = true;

        public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
        {
            if (transform.parent != null)
            {
                unsetParent = true;
                Rigidbody rigidBody = gameObject.AddComponent<Rigidbody>();
                rigidBody.isKinematic = false;
                rigidBody.useGravity = false;
                rigidBody.drag = 10f;
                rigidBody.angularDrag = 15f;
            }
            base.OnInteractableObjectGrabbed(e);
        }

        public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
        {
            base.OnInteractableObjectUngrabbed(e);
            if (unsetParent)
            {
                transform.parent = null;
            }
        }
    }
}
