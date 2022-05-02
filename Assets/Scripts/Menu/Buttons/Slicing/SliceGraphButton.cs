using AnalysisLogic;
using CellexalVR.AnalysisLogic;
using CellexalVR.General;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class SliceGraphButton : CellexalButton
    {
        protected override string Description => "Slice Graph";

        private SlicingMenu slicingMenu;


        protected override void Awake()
        {
            base.Awake();
            //SetButtonActivated(false);
            slicingMenu = GetComponentInParent<SlicingMenu>();
        }
        
        public override void Click()
        {
            if (!slicingMenu.GetComponentInParent<PointCloud>(true).gameObject.activeSelf)
            {
                controllerInside = false;
                return;
            }
            slicingMenu.SliceGraph();
        }
    }
}