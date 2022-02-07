using UnityEngine;
using System.Collections;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class ToggleSlicingMenuButton : CellexalButton
    {
        protected override string Description => "Hide/Show Slicing Menu";

        public SlicingMenu slicingMenu;

        private bool active;

        public override void Click()
        {
            slicingMenu.ToggleMenu(!active);
            active = !active;
        }

    }
}
