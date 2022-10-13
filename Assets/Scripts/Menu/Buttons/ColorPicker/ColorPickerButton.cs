using Assets.Scripts.Menu.Buttons.ColorPicker;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Menu.ColorPicker
{
    public class ColorPickerButton : CellexalVR.Menu.Buttons.CellexalButton
    {
        public ColorPickerPopout colorPicker;


        protected override string Description => "Click to open the color picker and choose a new color";

        public override void Click()
        {
            colorPicker.Open();
        }
    }
}