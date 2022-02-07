using UnityEngine;
using System.Collections;
using CellexalVR.Interaction;
using AnalysisLogic;

namespace CellexalVR.Spatial
{

    public class CullingWall : MonoBehaviour
    {
        public GameObject wall;
        public GameObject handle;
        public Vector3 sliderStartPosition;
        public Vector3 handleStartPosition;

        private MeshRenderer mr;
        private PointCloud parentPointCloud;
        private SliderController slider;

        private void Start()
        {
            mr = GetComponent<MeshRenderer>();
            parentPointCloud = GetComponentInParent<PointCloud>();
            slider = GetComponentInParent<SliderController>(true);
        }

        public void SetStartPosition()
        {
            if (slider == null)
                slider = GetComponentInParent<SliderController>(true);
            sliderStartPosition = slider.transform.localPosition;
            handleStartPosition = transform.localPosition;
            
        }

        public void Reset()
        {
            slider.transform.localPosition = sliderStartPosition;
            transform.localPosition = handleStartPosition;
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
