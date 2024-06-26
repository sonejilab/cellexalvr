﻿namespace CellexalVR.Menu.Buttons.Slicing
{
    public class ChangeSliceModeButton : CellexalButton
    {
        public SlicingMenu.SliceMode modeMenuToActivate;
        protected override string Description => "Switch to " + modeMenuToActivate;

        private SlicingMenu slicingMenu;

        protected override void Awake()
        {
            base.Awake();
            SetButtonActivated(false);
            slicingMenu = GetComponentInParent<SlicingMenu>();
        }
        
        public override void Click()
        {
            slicingMenu.ToggleMode(modeMenuToActivate, false);
            
            // TODO: Add multi user synch
            
        }
    }
}