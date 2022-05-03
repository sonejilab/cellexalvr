using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Interaction
{

    [CanSelectMultiple(true)]
    public class OffsetGrab : XRGrabInteractable
    {
        private Vector3 interactorPosition = Vector3.zero;
        private Quaternion interactorRotation = Quaternion.identity;
        private Vector3 interactorScale = Vector3.one;
        public ScaleGrab scaleGrab;

        private IXRSelectInteractor savedInteractor;
        private bool oldTrackposition;

        [SerializeField] private InputActionAsset actionAsset;
        [SerializeField] private InputActionReference action;


        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            StoreInteractor(args.interactorObject);
            MatchAttachmentPoints(args.interactorObject);
            base.OnSelectEntered(args);
        }

        protected override void OnSelectEntering(SelectEnterEventArgs args)
        {
            base.OnSelectEntering(args);
            if (scaleGrab && base.interactorsSelecting.Count == 2)
            {
                oldTrackposition = base.trackPosition;
                base.trackPosition = false;
                base.trackRotation = false;
                scaleGrab.doScale = true;
                scaleGrab.firstInteractor = savedInteractor;
                scaleGrab.secondInteractor = args.interactorObject;
                scaleGrab.InitialisePositions();
            }
            else
            {
                savedInteractor = args.interactorObject;
            }
        }

        private void StoreInteractor(IXRInteractor interactor)
        {
            Transform attachTransform = interactor.GetAttachTransform(this);
            interactorPosition = attachTransform.position;
            interactorRotation = attachTransform.rotation;
            interactorScale = attachTransform.localScale;
        }
        private void MatchAttachmentPoints(IXRInteractor interactor)
        {
            Transform attachTransform = interactor.GetAttachTransform(this);
            Transform interactorAttachTransform = GetAttachTransform(interactor);
            bool hasAttach = attachTransform != null;
            attachTransform.position = hasAttach ? interactorAttachTransform.position : transform.localPosition;
            attachTransform.rotation = hasAttach ? interactorAttachTransform.rotation : transform.localRotation;
            attachTransform.localScale = hasAttach ? interactorAttachTransform.localScale : transform.localScale;
        }

        protected override void OnSelectExiting(SelectExitEventArgs args)
        {
            if (scaleGrab && base.interactorsSelecting.Count == 2)
            {
                base.trackPosition = oldTrackposition;
                base.trackRotation = true;
                scaleGrab.doScale = false;
                if (scaleGrab.firstInteractor == args.interactorObject)
                {
                    savedInteractor = scaleGrab.secondInteractor;
                }
                else
                {
                    savedInteractor = scaleGrab.firstInteractor;
                }
                // positions and rotations have not been tracked since scaling begun, update them manually once.
                scaleGrab.firstInteractor.GetAttachTransform(args.interactableObject).transform.position = transform.position;
                scaleGrab.firstInteractor.GetAttachTransform(args.interactableObject).transform.rotation = transform.rotation;
                scaleGrab.secondInteractor.GetAttachTransform(args.interactableObject).transform.position = transform.position;
                scaleGrab.secondInteractor.GetAttachTransform(args.interactableObject).transform.rotation = transform.rotation;
            }
            base.OnSelectExiting(args);
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            ClearInteractor(args.interactorObject);
            ResetAttachmentPoints(args.interactorObject);
            base.OnSelectExited(args);
        }

        private void ClearInteractor(IXRInteractor interactor)
        {
            interactorPosition = Vector3.zero;
            interactorRotation = Quaternion.identity;
            interactorScale = Vector3.one;
        }

        private void ResetAttachmentPoints(IXRInteractor interactor)
        {
            Transform attachTransform = interactor.GetAttachTransform(this);
            attachTransform.localPosition = interactorPosition;
            attachTransform.localRotation = interactorRotation;
            attachTransform.localScale = interactorScale;
        }


    }
}