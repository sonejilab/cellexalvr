using CellexalVR.General;
using CellexalVR.Menu.Buttons;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Menu.Buttons.ColorPicker
{
    public class AddSelectionColorButton : CellexalButton
    {

        protected override string Description => "Add Selection Color";

        public override void Click()
        {
            referenceManager.settingsMenu.AddSelectionColor();
            referenceManager.colorPickerSubMenu.AddSelectionColorButton(CellexalConfig.Config.SelectionToolColors[^1], CellexalConfig.Config.SelectionToolColors.Length - 1);
        }

    }
}
