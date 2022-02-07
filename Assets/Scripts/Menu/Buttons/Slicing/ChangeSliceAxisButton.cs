using UnityEngine;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class ChangeSliceAxisButton : CellexalButton
    {
        public SlicingMenu.SliceAxis axisToSlice;

        protected override string Description => "Switch to slice " + axisToSlice;
        [SerializeField] private bool startAsActive;

        private SlicingMenu slicingMenu;

        protected override void Awake()
        {
            base.Awake();
            slicingMenu = GetComponentInParent<SlicingMenu>();
            if (startAsActive)
            {
                Click();
            }
        }

        public override void Click()
        {
            // change slice axis
            slicingMenu.ChangeAxis(axisToSlice);
            SetButtonActivated(false);
            // TODO: Add multi user synch

        }
    }
}