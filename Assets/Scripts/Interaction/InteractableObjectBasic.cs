using UnityEngine;
using Valve.VR;

namespace CellexalVR.Interaction
{
    using UnityEngine;
    using Valve.VR.InteractionSystem;

    //-------------------------------------------------------------------------
    [RequireComponent(typeof(Interactable))]
    public class InteractableObjectBasic : MonoBehaviour
    {
        public bool isGrabbed;
        public bool hideControllerOnAttach;

        public struct InteractableObjectEventArgs
        {
            public GameObject interactingObject;
        }
        
        public delegate void InteractableObjectEventHandler(object sender, Hand hand);

        public event InteractableObjectEventHandler InteractableObjectGrabbed;
        public event InteractableObjectEventHandler InteractableObjectUnGrabbed;

        private Hand.AttachmentFlags attachmentFlags =
            Hand.defaultAttachmentFlags & (~Hand.AttachmentFlags.SnapOnAttach) & (~Hand.AttachmentFlags.DetachOthers) & (~Hand.AttachmentFlags.VelocityMovement);

        private Interactable interactable;

        private Transform prevParent; 
        private Transform currParrent;
        private Vector3 prevPos;
        private Quaternion prevRot;
        private Vector3 prevScale;
        // private bool previousKinematicState;
        // private bool previousIsGrabbable;


        //-------------------------------------------------
        protected virtual void Awake()
        {
            interactable = GetComponent<Interactable>();
        }

        private void SavePreviousState(Transform t, bool isKinematic, bool isGrabbable)
        {
            if (t.parent == null)
            {
                currParrent = t.parent;
                return;
            }
            prevParent = t.parent;
            prevPos = t.localPosition;
            prevRot = t.localRotation;
            prevScale = t.localScale;
        }

        public Transform GetPreviousParent()
        {
            return prevParent;
        }
        
        public Transform GetCurrentParent()
        {
            return currParrent;
        }

        public Vector3 GetPreviousPosition()
        {
            return prevPos;
        }
        public Quaternion GetPreviousRotation()
        {
            return prevRot;
        }
        public Vector3 GetPreviousScale()
        {
            return prevScale;
        }
        
        //-------------------------------------------------
        // Called when a Hand starts hovering over this object
        //-------------------------------------------------
        private void OnHandHoverBegin(Hand hand)
        {
        }


        //-------------------------------------------------
        // Called when a Hand stops hovering over this object
        //-------------------------------------------------
        private void OnHandHoverEnd(Hand hand)
        {
        }


        //-------------------------------------------------
        // Called every Update() while a Hand is hovering over this object
        //-------------------------------------------------
        private void HandHoverUpdate(Hand hand)
        {
            GrabTypes startingGrabType = hand.GetGrabStarting();
            bool isGrabEnding = hand.IsGrabEnding(this.gameObject);

            if (interactable.attachedToHand == null && startingGrabType != GrabTypes.None)
            {
                // Call this to continue receiving HandHoverUpdate messages,
                // and prevent the hand from hovering over anything else
                hand.HoverLock(interactable);
                
                // Save information about object before attaching to hand.
                SavePreviousState(transform, false, true);
                
                // Attach this object to the hand
                hand.AttachObject(gameObject, startingGrabType, attachmentFlags);
            }
            else if (isGrabEnding) 
            {
                // Detach this object from the hand
                hand.DetachObject(gameObject);

                
                
                // Call this to undo HoverLock
                hand.HoverUnlock(interactable);

                // Restore position/rotation
                //transform.position = oldPosition;
                //transform.rotation = oldRotation;
            }
        }


        //-------------------------------------------------
        // Called when this GameObject becomes attached to the hand
        //-------------------------------------------------
        private void OnAttachedToHand(Hand hand)
        {
            isGrabbed = true;
            InteractableObjectGrabbed?.Invoke(this, hand);
            if (hideControllerOnAttach)
            {
                ToggleController(false, hand);
            }
        }


        //-------------------------------------------------
        // Called when this GameObject is detached from the hand
        //-------------------------------------------------
        private void OnDetachedFromHand(Hand hand)
        {
            isGrabbed = false;
            InteractableObjectUnGrabbed?.Invoke(this, hand);
            if (hideControllerOnAttach)
            {
                ToggleController(true, hand);
            }
        }


        private void ToggleController(bool show, Hand hand)
        {
            hand.GetComponentInChildren<SteamVR_RenderModel>(true).gameObject.SetActive(show);
        }


        //-------------------------------------------------
        // Called every Update() while this GameObject is attached to the hand
        //-------------------------------------------------
        protected virtual void HandAttachedUpdate(Hand hand)
        {
        }

        private bool lastHovering = false;

        private void Update()
        {
            if (interactable.isHovering != lastHovering) //save on the .tostrings a bit
            {
                lastHovering = interactable.isHovering;
            }
        }


        //-------------------------------------------------
        // Called when this attached GameObject becomes the primary attached object
        //-------------------------------------------------
        private void OnHandFocusAcquired(Hand hand)
        {
        }


        //-------------------------------------------------
        // Called when another attached GameObject becomes the primary attached object
        //-------------------------------------------------
        private void OnHandFocusLost(Hand hand)
        {
        }

        protected virtual void OnInteractableObjectGrabbed(Hand hand)
        {
        }

        protected virtual void OnInteractableObjectUnGrabbed(Hand hand)
        {
        }
    }
}