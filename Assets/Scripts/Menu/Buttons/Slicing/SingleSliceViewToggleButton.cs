using UnityEngine;
using System.Collections;
using CellexalVR.Spatial;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class SingleSliceViewToggleButton : SliderButton
    {
        public GameObject menuToToggle;

        private SlicerBox slicerBox;

        protected override string Description => "Toggle single slice view mode";

        protected override void Awake()
        {
            base.Awake();
            slicerBox = GetComponentInParent<SlicerBox>();
        }

        protected override void ActionsAfterSliding()
        {
            slicerBox.SingleSliceViewMode(currentState, 2);
            if (menuToToggle != null)
            {
                menuToToggle.SetActive(currentState);
            }
        }

    }

}