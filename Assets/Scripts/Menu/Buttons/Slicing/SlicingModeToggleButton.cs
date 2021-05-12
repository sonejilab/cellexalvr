using UnityEngine;
using System.Collections;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class SlicingModeToggleButton : SliderButton
    {
        public SlicingMenu.SliceMode modeMenuToActivate;
        protected override string Description => "Switch to " + modeMenuToActivate;

        private SlicingMenu slicingMenu;

        protected override void Awake()
        {
            base.Awake();
            slicingMenu = GetComponentInParent<SlicingMenu>();
        }

        protected override void ActionsAfterSliding()
        {
            slicingMenu.ToggleMode(modeMenuToActivate, currentState);
        }

    }

}