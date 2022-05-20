using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace CellexalVR.Spatial
{
    /// <summary>
    /// Class to represent a Region of Interest (ROI) image such as what happens when selected. 
    /// An roi image is linked to a scan and 1 or more aoi images.
    /// </summary>
    public class GeoMXROISlide : GeoMXSlide
    {
        public string scanID;
        public string roiID;
        public string[] aoiIDs;

        private bool selected;

        /// <summary>
        /// Select/Unselect this image. Highlights/Unhighlights the image as well as spawns/destroys the linked aoi images.
        /// </summary>
        public override void Select()
        {
            if (selected)
            {
                selected = false;
                imageHandler.UnSelectROI(roiID, true);
                UnHighlight();
            }
            else
            {
                selected = true;
                imageHandler.SpawnAOIImages(scanID, aoiIDs, roiID);
                Highlight();
            }
        }

        public override void SelectCells(int group){}

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

        public override void OnRaycastHit() {}
    }
}