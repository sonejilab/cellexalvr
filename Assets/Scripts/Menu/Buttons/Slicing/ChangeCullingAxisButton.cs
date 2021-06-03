using CellexalVR.Spatial;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class ChangeCullingAxisButton : CellexalButton
    {
        public SlicingMenu.SliceAxis axisToCull;

        protected override string Description => "Switch to cull " + axisToCull;
        [SerializeField] private bool startAsActive;

        private SlicerBox slicerBox;

        protected override void Awake()
        {
            base.Awake();
            slicerBox = GetComponentInParent<SlicerBox>();
            if (startAsActive)
            {
                Click();
            }
        }

        public override void Click()
        {
            slicerBox.SingleSliceViewMode(true, (int)axisToCull);
            SetButtonActivated(false);
            // TODO: Add multi user synch
        }
    }
}