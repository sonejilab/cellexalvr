using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Interaction
{
    [RequireComponent(typeof(XRGrabInteractable))]
    public class ScaleGrab : MonoBehaviour
    {
        public IXRSelectInteractor firstInteractor;
        public IXRSelectInteractor secondInteractor;
        public bool doScale;

        private Vector3 interactorPosition = Vector3.zero;
        private Quaternion interactorRotation = Quaternion.identity;
        private Vector3 interactorScale = Vector3.one;

        private Vector3 a;
        private Vector3 localA;
        private Vector3 b;

        private void Update()
        {
            if (doScale)
            {
                UniformScale();
            }
        }

        private void ClearInteractor(XRBaseInteractor interactor)
        {
            interactorPosition = Vector3.zero;
            interactorRotation = Quaternion.identity;
            interactorScale = Vector3.one;
            firstInteractor = secondInteractor = null;
        }

        private void ResetAttachmentPoints(XRBaseInteractor interactor)
        {
            interactor.attachTransform.localPosition = interactorPosition;
            interactor.attachTransform.localRotation = interactorRotation;
            interactor.attachTransform.localScale = interactorScale;
        }

        public void InitialisePositions()
        {
            a = firstInteractor.transform.position;
            localA = transform.InverseTransformPoint(a);
            b = secondInteractor.transform.position;
        }

        protected void UniformScale()
        {
            Vector3 aa = firstInteractor.transform.position;
            Vector3 bb = secondInteractor.transform.position;

            float c = (a - b).magnitude;
            float cc = (aa - bb).magnitude;

            Vector3 axis = -Vector3.Cross(bb - aa, b - a);
            float angle = Vector3.Angle(bb - aa, b - a);

            transform.RotateAround(aa, axis, angle);

            Vector3 newScale = transform.localScale * (cc / c);
            transform.localScale = newScale;

            Vector3 newWorldaa = transform.TransformPoint(localA);
            transform.Translate(aa - newWorldaa, Space.World);

            a = aa;
            b = bb;

        }
    }
}
