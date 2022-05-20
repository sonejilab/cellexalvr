using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CellexalVR.Spatial
{
    /// <summary>
    /// Class to represent a Scan image such as what happens when selected. 
    /// An scan image is linked to several roi images which are in turn is linked to aoi images.
    /// </summary>
    public class GeoMXScanSlide : GeoMXSlide
    {
        public string scanID;
        public string[] rois;

        private bool selected;

        /// <summary>
        /// Selecting/Unselecting a scan spawns the linked roi images and highlights the image.
        /// </summary>
        public override void Select()
        {
            if (selected)
            {
                selected = false;
                imageHandler.UnSelectScan(scanID, true);
                UnHighlight();

            }
            else
            {
                selected = true;
                imageHandler.SpawnROIImages(scanID, rois);
                Highlight();
            }
        }

        public override void SelectCells(int group)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Detach from the slide scroller so you can freely move it around.
        /// </summary>
        public override void Detach()
        {
            base.Detach();
        }

        /// <summary>
        /// Reattaches to its position in the slide scroller.
        /// </summary>
        public override void Reattach()
        {
            base.Reattach();

        }
    }
}