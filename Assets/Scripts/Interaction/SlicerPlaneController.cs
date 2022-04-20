using AnalysisLogic;
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
        private IXRSelectInteractor interactor;
        private bool shouldGetHandPosition;
        private float currentValue;
        private Material mat;
        private Transform boxParent;
        private PointCloud pointCloud => GetComponentInParent<PointCloud>();

        private void Start()
        {
            mat = slicePlane.GetComponent<Renderer>().material;
            boxParent = transform.parent;
        }

        private void OnEnable()
        {
            interactable.selectEntered.AddListener(OnGrabbed);
            interactable.selectExited.AddListener(OnUnGrabbed);
            interactable.hoverEntered.AddListener(OnHoverEnter);
            interactable.hoverExited.AddListener(OnHoverExit);
            //interactable.colliders.Add(interactable.GetComponent<SphereCollider>());
        }

        private void OnHoverEnter(HoverEnterEventArgs args)
        {
            pointCloud.GetComponent<BoxCollider>().enabled = false;
        }

        private void OnHoverExit(HoverExitEventArgs args)
        {
            pointCloud.GetComponent<BoxCollider>().enabled = true;
        }

        private void OnGrabbed(SelectEnterEventArgs args)
        {
            interactor = args.interactorObject;
            shouldGetHandPosition = true;
        }

        private void OnUnGrabbed(SelectExitEventArgs args)
        {
            shouldGetHandPosition = false;
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
                joint.rotation = interactable.transform.rotation;
            }
        }


    }

}