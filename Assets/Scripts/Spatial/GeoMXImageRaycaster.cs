using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CellexalVR.Spatial
{
    /// <summary>
    /// Class to handle the rayasting of the different geo mx images.
    /// The selection of the scans, roi, aois are handled via raycasting (and trigger click).
    /// The images are all assumed to have a collider on the EnvironmentButtonLayer and a GeoMXSlide component.
    /// </summary>
    public class GeoMXImageRaycaster : MonoBehaviour
    {
        private GeoMXImageHandler imageHandler;
        private bool block = true;
        private GeoMXSlide currentSlideHit;

        private void Start()
        {
            imageHandler = GetComponent<GeoMXImageHandler>();
            CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);
            CellexalEvents.LoadingImages.AddListener(() => block = true);
            CellexalEvents.ImagesLoaded.AddListener(() => block = false);
        }

        /// <summary>
        /// When trigger click is called ray cast and see if an image is it. If it is the image is selected.
        /// </summary>
        private void OnTriggerClick()
        {
            if (block)
                return;
            Transform rLaser = ReferenceManager.instance.rightLaser.transform;
            Physics.Raycast(rLaser.position, rLaser.forward, out RaycastHit hit);
            if (hit.collider != null)
            {
                GeoMXSlide slide = hit.collider.GetComponent<GeoMXSlide>();
                if (slide != null && ReferenceManager.instance.rightLaser.enabled)
                {
                    slide.Select();
                }
            }
        }

        private void Update()
        {
            Raycast();
        }
        
        /// <summary>
        /// Raycast and see if image is hit. Displays the name of the image (if hit).
        /// </summary>
        private void Raycast()
        {
            if (block)
                return;
            Transform rLaser = ReferenceManager.instance.rightLaser.transform;
            Physics.Raycast(rLaser.position, rLaser.forward, out RaycastHit hit, 10, 1 << LayerMask.NameToLayer("EnvironmentButtonLayer"));
            if (hit.collider)
            {
                GeoMXSlide slide = hit.collider.GetComponent<GeoMXSlide>();
                if (slide != null)
                {
                    slide.ShowName();
                    if (currentSlideHit == slide)
                        return;
                    slide.OnRaycastHit();
                    currentSlideHit = slide;
                }
                else
                {
                    imageHandler.ResetDisplayName();
                    currentSlideHit = null;
                }
            }
            else
            {
                imageHandler.ResetDisplayName();
                currentSlideHit = null;
            }
        }
    }
}
