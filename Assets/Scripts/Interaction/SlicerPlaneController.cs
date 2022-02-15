using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Interaction
{

    public class SlicerPlaneController : MonoBehaviour
    {
        [SerializeField] private Transform joint;
        [SerializeField] private Transform slicer;
        [SerializeField] private float minPos;
        [SerializeField] private float maxPos;
        [SerializeField] private GameObject slicePlane;
        [SerializeField] private OffsetGrab interactable;

        private XRBaseInteractor interactor;
        private float startPosition;
        private Vector3 startAngle;
        private bool shouldGetHandPosition;
        private bool requiresStartAngle;

        private float currentValue;
        private Material mat;
        private Transform boxParent;

        private void Start()
        {
            mat = slicePlane.GetComponent<Renderer>().material;
            boxParent = transform.parent;
        }

  

        private void OnEnable()
        {
            interactable.selectEntered.AddListener(OnGrabbed);
            interactable.selectExited.AddListener(OnUnGrabbed);
        }

        private void OnGrabbed(SelectEnterEventArgs args)
        {
            interactor = args.interactor;
            startAngle = Quaternion.identity.eulerAngles;
            shouldGetHandPosition = true;

        }

        private void OnUnGrabbed(SelectExitEventArgs args)
        {
            shouldGetHandPosition = false;
            requiresStartAngle = true;
            interactable.transform.localPosition = joint.transform.localPosition;
        }

        private void Update()
        {
            mat.SetMatrix("_BoxMatrix", boxParent.worldToLocalMatrix);
            if (shouldGetHandPosition)
            {
                Vector3 handPosInLocalSpace = slicer.InverseTransformPoint(interactor.transform.position);
                joint.localPosition = new Vector3(
                    Mathf.Clamp(handPosInLocalSpace.x, minPos, maxPos),
                    Mathf.Clamp(handPosInLocalSpace.y, minPos, maxPos),
                    Mathf.Clamp(handPosInLocalSpace.z, minPos, maxPos)
                );
                joint.localRotation = interactable.transform.localRotation;

                //var rotationAngle = GetInteractorRotation();
                //GetRotationDistance(rotationAngle);
                //joint.transform.eulerAngles = rotationAngle;
            }
        }

        private Vector3 GetInteractorRotation() => interactor.GetComponent<Transform>().eulerAngles;

        private void GetRotationDistance(Vector3 currentAngle)
        {
            if (!requiresStartAngle)
            {
                var xDiff = Mathf.Abs(startAngle.x - currentAngle.x);
                var yDiff = Mathf.Abs(startAngle.y - currentAngle.y);
                var zDiff = Mathf.Abs(startAngle.z - currentAngle.z);

                print($"{xDiff}, {yDiff}, {zDiff}");
                joint.localEulerAngles = new Vector3(joint.localEulerAngles.x + xDiff, joint.localEulerAngles.y + yDiff, joint.localEulerAngles.z + zDiff);
                startAngle = currentAngle;
            }

            else
            {
                requiresStartAngle = false;
                startAngle = currentAngle;
            }
        }

    }

}