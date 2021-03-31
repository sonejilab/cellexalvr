using UnityEngine;
using System.Collections;

namespace CellexalVR.Menu.Buttons.Slicing
{
    public class ToggleSlicingMenuButton : CellexalButton
    {
        protected override string Description => "Hide/Show Slicing Menu";

        public SlicingMenu slicingMenu;

        private bool active = true;

        public override void Click()
        {
            if (active)
            {
                StartCoroutine(slicingMenu.MinimizeMenu());
            }
            else
            {
                StartCoroutine(slicingMenu.MaximizeMenu());
            }
            active = !active;
        }

    }
}
