using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Interaction
{

    [CanSelectMultiple(true)]
    public class OffsetGrab : XRGrabInteractable
    {
        private Vector3 interactorPosition = Vector3.zero;
        private Quaternion interactorRotation = Quaternion.identity;
        private Vector3 interactorScale = Vector3.one;

        protected override void OnSelectEntered(SelectEnterEventArgs args)
        {
            StoreInteractor(args.interactor);
            MatchAttachmentPoints(args.interactor);
            base.OnSelectEntered(args);
            
        }

        private void StoreInteractor(XRBaseInteractor interactor)
        {
            interactorPosition = interactor.attachTransform.position;
            interactorRotation = interactor.attachTransform.rotation;
            interactorScale = interactor.attachTransform.localScale;
        }
        private void MatchAttachmentPoints(XRBaseInteractor interactor)
        {
            bool hasAttach = attachTransform != null;
            interactor.attachTransform.position = hasAttach ? attachTransform.position : transform.localPosition;
            interactor.attachTransform.rotation = hasAttach ? attachTransform.rotation : transform.localRotation;
            interactor.attachTransform.localScale = hasAttach ? attachTransform.localScale : transform.localScale;
        }

        protected override void OnSelectExited(SelectExitEventArgs args)
        {
            ClearInteractor(args.interactor);
            ResetAttachmentPoints(args.interactor);
            base.OnSelectExited(args);
        }

        private void ClearInteractor(XRBaseInteractor interactor)
        {
            interactorPosition = Vector3.zero;
            interactorRotation = Quaternion.identity;
            interactorScale = Vector3.one;
        }

        private void ResetAttachmentPoints(XRBaseInteractor interactor)
        {
            interactor.attachTransform.localPosition = interactorPosition;
            interactor.attachTransform.localRotation = interactorRotation;
            interactor.attachTransform.localScale = interactorScale;
        }


    }
}