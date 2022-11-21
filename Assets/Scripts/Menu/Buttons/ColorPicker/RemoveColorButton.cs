using Assets.Scripts.Menu.SubMenus;
using CellexalVR.Menu.Buttons;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Menu.Buttons.ColorPicker
{
    public class RemoveColorButton : CellexalButton
    {
        public ColorPickerPopout colorPickerPopout;
        protected override string Description => "Remove Color";

        public override void Click()
        {
            colorPickerPopout.RemoveColor();
        }
    }
}