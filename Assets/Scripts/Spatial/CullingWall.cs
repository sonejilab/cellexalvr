using UnityEngine;
using System.Collections;
using CellexalVR.Interaction;
using Valve.VR.InteractionSystem;
using AnalysisLogic;

namespace CellexalVR.Spatial
{

    public class CullingWall : MonoBehaviour
    {
        public GameObject wall;
        public GameObject handle; 

        private MeshRenderer mr;
        private InteractableObjectOneAxis interactable;
        private PointCloud parentPointCloud;

        private void Start()
        {
            interactable = GetComponent<InteractableObjectOneAxis>();
            mr = GetComponent<MeshRenderer>();
            interactable.InteractableObjectGrabbed += OnGrabbed;
            interactable.InteractableObjectUnGrabbed += OnUnGrabbed;
            parentPointCloud = GetComponentInParent<PointCloud>();
        }

        public void SetStartPosition()
        {
            transform.localScale = new Vector3(1f / transform.lossyScale.x, 1f / transform.lossyScale.y, 1f / transform.lossyScale.z);
            var interactable = GetComponent<InteractableObjectOneAxis>();
            interactable.startPosition = transform.localPosition;
            interactable.maxAxisValue = Mathf.Abs(transform.localPosition[(int)interactable.movableAxis]);
            interactable.minAxisValue = -Mathf.Abs(transform.localPosition[(int)interactable.movableAxis]);
        }

        private void OnGrabbed(object sender, Hand hand)
        {
            parentPointCloud.GetComponent<InteractableObjectBasic>().isGrabbable = false;
            wall.SetActive(true);
        }

        private void OnUnGrabbed(object sender, Hand hand)
        {
            parentPointCloud.GetComponent<InteractableObjectBasic>().isGrabbable = true;
            wall.SetActive(false);
        }


        //private void OnTriggerEnter(Collider other)
        //{
        //    if (other.CompareTag("Player"))
        //    {
        //        //mr.enabled = true;
        //        wall.SetActive(true);
        //    }
        //}

        //private void OnTriggerExit(Collider other)
        //{
        //    if (!interactable.isGrabbed && other.CompareTag("Player"))
        //    {
        //        //mr.enabled = false;
        //        wall.SetActive(false);
        //    }
        //}

    }
}
