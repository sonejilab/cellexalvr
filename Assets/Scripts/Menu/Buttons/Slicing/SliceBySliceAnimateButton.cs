using CellexalVR.AnalysisLogic;
using CellexalVR.Spatial;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class SliceBySliceAnimateButton : CellexalButton
    {
        protected override string Description => "Slice Animation";

        private SlicerBox slicerBox;


        protected override void Awake()
        {
            base.Awake();
            //SetButtonActivated(false);
            slicerBox = GetComponentInParent<SlicerBox>();
        }

        public override void Click()
        {
            slicerBox.SliceBySliceAnimation();
            // TODO: Add multi-user functionality.
        }
    }
}
