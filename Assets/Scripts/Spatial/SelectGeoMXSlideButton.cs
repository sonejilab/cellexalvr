using CellexalVR.Menu.Buttons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CellexalVR.Spatial
{
    /// <summary>
    /// Select image slide that calls the parent class select cells.
    /// If it is an aoi image it selects the corresponding cell in the graph.
    /// </summary>
    public class SelectGeoMXSlideButton : CellexalButton
    {
        public int group;
        protected override string Description => "";

        private GeoMXSlide slide;

        private void Start()
        {
            slide = GetComponentInParent<GeoMXSlide>();
        }

        public override void Click()
        {
            slide.SelectCells(group);
        }

    }

}