using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CellexalVR.Spatial
{

    public class GeoMXAOISlide : GeoMXSlide
    {
        public string aoiID;

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
        public override void Select()
        {
            // HighLight points?
        }
        public override void SelectCells(int group)
        {
            int cellID = imageHandler.GetCellFromAoiID(aoiID).id;
            var graph = imageHandler.referenceManager.graphManager.Graphs[0];
            var gPoint = graph.FindGraphPoint(cellID.ToString());
            imageHandler.referenceManager.selectionManager.AddGraphpointToSelection(gPoint, group, false);
            highlight.GetComponent<MeshRenderer>().material.color = CellexalConfig.Config.SelectionToolColors[group];
            Highlight();
        }


    }
}
