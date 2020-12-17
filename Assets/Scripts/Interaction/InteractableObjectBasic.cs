using Valve.VR;

namespace CellexalVR.Interaction
{
    using UnityEngine;
    using Valve.VR.InteractionSystem;

    //-------------------------------------------------------------------------
    [RequireComponent(typeof(Interactable))]
    public class InteractableObjectBasic : MonoBehaviour
    {
        public bool allowLeftHandGrab = true;
        public bool isGrabbable = true;
        public bool isGrabbed;
        public bool hideControllerOnAttach;
        public bool allowTwoHandGrab = true;

        private Transform primaryGrabbingObject;
        private Transform secondaryGrabbingObject;
        private Vector3 primaryGrabbingStartPosition;
        private Vector3 secondaryGrabbingStartPosition;


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

        [SerializeField] private Transform prevParent;
        [SerializeField] private Transform currParrent;
        [SerializeField] private Vector3 prevPos;
        [SerializeField] private Quaternion prevRot;

        private Vector3 prevScale;
        // private bool previousKinematicState;
        // private bool previousIsGrabbable;


        //-------------------------------------------------
        protected virtual void Awake()
        {
            interactable = GetComponent<Interactable>();
        }

        public void SavePreviousState(Transform t, bool isGrabbable)
        {
            if (t.parent == null)
            {
                currParrent = t.parent;
            }
            // sometimes it seems to parent to right hand. cant figure out why so i added the following check
            else if (t.parent.GetComponent<Hand>() != null && t.parent.GetComponent<Hand>().handType == SteamVR_Input_Sources.RightHand)
            {
                currParrent = null;
            }
            else
            {
                prevParent = t.parent;
                prevPos = t.localPosition;
                prevRot = t.localRotation;
                prevScale = t.localScale;
            }
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
            if (!isGrabbable || (!allowLeftHandGrab && hand.handType != SteamVR_Input_Sources.RightHand)) return;
            GrabTypes startingGrabType = hand.GetGrabStarting();
            GrabTypes endingGrabType = hand.GetGrabEnding();
            bool isGrabEnding = hand.IsGrabEnding(this.gameObject);
            HandleGrabInput(hand, startingGrabType, endingGrabType, isGrabEnding);
        }

        public void HandleGrabInput(Hand hand, GrabTypes startingGrabType, GrabTypes endingGrabType, bool isGrabEnding)
        {
            if (interactable.attachedToHand == null && startingGrabType == GrabTypes.Grip)
            {
                primaryGrabbingObject = hand.transform;
                // Call this to continue receiving HandHoverUpdate messages,
                // and prevent the hand from hovering over anything else
                hand.HoverLock(interactable);

                // Save information about object before attaching to hand.
                SavePreviousState(transform, true);

                // Attach this object to the hand
                hand.AttachObject(gameObject, startingGrabType, attachmentFlags);
            }
            else if (isGrabEnding)
            {
                primaryGrabbingObject = secondaryGrabbingObject = null;
                // Detach this object from the hand
                hand.DetachObject(gameObject);

                // Call this to undo HoverLock
                hand.HoverUnlock(interactable);
            }
            // second hand grabbing 
            else if (allowTwoHandGrab && interactable.attachedToHand == hand.otherHand && startingGrabType == GrabTypes.Grip)
            {
                secondaryGrabbingObject = hand.transform;
                primaryGrabbingStartPosition = interactable.attachedToHand.transform.position;
                secondaryGrabbingStartPosition = hand.transform.position;
                if (hideControllerOnAttach)
                {
                    ToggleController(false, hand);
                }
            }

            // second hand released object. stop scaling
            else if (primaryGrabbingObject != null && secondaryGrabbingObject != null && endingGrabType == GrabTypes.Grip)
            {
                secondaryGrabbingObject = null;
                if (hideControllerOnAttach)
                {
                    ToggleController(true, hand);
                }
            }
        }


        //-------------------------------------------------
        // Called when this GameObject becomes attached to the hand
        //-------------------------------------------------
        private void OnAttachedToHand(Hand hand)
        {
            if (hideControllerOnAttach)
            {
                ToggleController(false, hand);
            }

            isGrabbed = true;
            InteractableObjectGrabbed?.Invoke(this, hand);
        }


        //-------------------------------------------------
        // Called when this GameObject is detached from the hand
        //-------------------------------------------------
        private void OnDetachedFromHand(Hand hand)
        {
            if (hideControllerOnAttach)
            {
                ToggleController(true, hand);
            }

            isGrabbed = false;
            InteractableObjectUnGrabbed?.Invoke(this, hand);
        }


        private static void ToggleController(bool show, Hand hand)
        {
            hand.GetComponentInChildren<SteamVR_RenderModel>(true).gameObject.SetActive(show);
        }


        //-------------------------------------------------
        // Called every Update() while this GameObject is attached to the hand
        //-------------------------------------------------
        protected virtual void HandAttachedUpdate(Hand hand)
        {
            if (primaryGrabbingObject == null || secondaryGrabbingObject == null) return;
            UniformScale();
        }

        protected void UniformScale()
        {
            Vector3 aa = primaryGrabbingObject.transform.position;
            Vector3 bb = secondaryGrabbingObject.transform.position;
            float c = (primaryGrabbingStartPosition - secondaryGrabbingStartPosition).magnitude;
            float cc = (aa - bb).magnitude;

            Vector3 axis = -Vector3.Cross(bb - aa, secondaryGrabbingStartPosition - primaryGrabbingStartPosition);
            float angle = Vector3.Angle(bb - aa, secondaryGrabbingStartPosition - primaryGrabbingStartPosition);

            transform.RotateAround(aa, axis, angle);

            Vector3 newScale = transform.localScale * (cc / c);
            if (Vector3.Distance(newScale, transform.localScale) > 1f) return; // if something went wrong (e.g. controller stopped tracking) dont scale. 
            transform.localScale = newScale;

            primaryGrabbingStartPosition = aa;
            secondaryGrabbingStartPosition = bb;
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