using CellexalVR.Menu.Buttons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CellexalVR.Spatial
{

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