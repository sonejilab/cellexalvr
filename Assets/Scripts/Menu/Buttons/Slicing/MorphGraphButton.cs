using CellexalVR.AnalysisLogic;
using CellexalVR.Spatial;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class MorphGraphButton : CellexalButton
    {
        protected override string Description => "Morph Graph";

        private SlicerBox slicerBox;


        protected override void Awake()
        {
            base.Awake();
            //SetButtonActivated(false);
            slicerBox = GetComponentInParent<SlicerBox>();
        }

        public override void Click()
        {
            slicerBox.MorphGraph();

            // TODO: Add multi-user functionality.
        }
    }
}