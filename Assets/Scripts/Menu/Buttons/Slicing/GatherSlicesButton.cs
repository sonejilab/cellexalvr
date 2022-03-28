using CellexalVR.Spatial;
using Unity.Entities;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class GatherSlicesButton : CellexalButton
    {
        protected override string Description => "Gather Slices to Parent";

        //private SliceManager sliceManager;

        public GraphSlice graphSlice;


        protected override void Awake()
        {
            base.Awake();
            graphSlice = GetComponentInParent<GraphSlice>();
            if (graphSlice.parentSlice == graphSlice)
            {
                SetButtonActivated(false);
            }
        }
        
        public override void Click()
        {
            if (!graphSlice.gameObject.activeSelf)
            {
                controllerInside = false;
                return;
            }
            graphSlice.parentSlice.ActivateSlices(false);
        }
    }
}