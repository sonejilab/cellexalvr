using UnityEngine;
using System.Collections;
using CellexalVR.Interaction;
using AnalysisLogic;

namespace CellexalVR.Spatial
{
    /// <summary>
    /// Class that handles the positioning of the culling walls. Culling walls are used to cull the pointclouds. Points on one side of the wall will be culled.
    /// </summary>
    public class CullingWall : MonoBehaviour
    {
        public GameObject wall;
        public GameObject handle;
        public Vector3 sliderStartPosition;
        public Vector3 handleStartPosition;

        private SliderController slider;

        private void Start()
        {
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

    }
}
