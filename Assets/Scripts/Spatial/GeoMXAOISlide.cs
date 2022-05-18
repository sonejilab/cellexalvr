using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CellexalVR.Spatial
{
    /// <summary>
    /// Class represents on Area of Illumination (AOI) image. Each aoi image corresponds to one cell in the graph.
    /// </summary>
    public class GeoMXAOISlide : GeoMXSlide
    {
        public string aoiID;
        public string scanID;

        protected override void Start()
        {
            base.Start();
            var buttons = GetComponentsInChildren<SelectGeoMXSlideButton>();
            for (int i = 0; i < buttons.Length; i++)
            {
                Color c = CellexalConfig.Config.SelectionToolColors[i];
                var button = buttons[i];
                button.GetComponent<MeshRenderer>().material.color = c;
                button.meshStandardColor = c;
                button.group = i;
            }
        }
        public override void Select() {}

        /// <summary>
        /// Select the cell with the same id as this aoi image. This highlights the graph point as well as this image.
        /// </summary>
        /// <param name="group"></param>
        public override void SelectCells(int group)
        {
            int cellID = GeoMXImageHandler.instance.GetCellFromAoiID(aoiID).id;
            var graph = ReferenceManager.instance.graphManager.Graphs[0];
            var gPoint = graph.FindGraphPoint(cellID.ToString());
            ReferenceManager.instance.selectionManager.AddGraphpointToSelection(gPoint, group, false);
            highlight.GetComponent<MeshRenderer>().material.color = CellexalConfig.Config.SelectionToolColors[group];
            Highlight();
        }


    }
}
