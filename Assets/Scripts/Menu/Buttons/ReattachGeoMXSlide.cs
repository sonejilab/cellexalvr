using CellexalVR.Spatial;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CellexalVR.Menu.Buttons
{

    public class ReattachGeoMXSlide : CellexalButton
    {
        private GeoMXSlide slide;
        protected override string Description => "";

        private void Start()
        {
            slide = GetComponentInParent<GeoMXSlide>();
            descriptionText = slide.imageHandler.GetComponentInChildren<TextMeshPro>();
        }

        public override void Click()
        {
            slide.Reattach();
        }
    }
}
