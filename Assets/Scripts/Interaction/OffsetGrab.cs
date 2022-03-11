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
            StoreInteractor(args.interactorObject);
            MatchAttachmentPoints(args.interactorObject);
            base.OnSelectEntered(args);

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