using CellexalVR.DesktopUI;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.ColorPicker
{
    public class ColorPickerButton : CellexalButton
    {
        public ColorPickerPopout colorPicker;
        public GameObject highlightGameObject;
        public ColorPickerButtonBase colorPickerButtonBase;

        protected override string Description => "Click to open the color picker and choose a new color";

        public override void Click()
        {
            colorPicker.Open(this);
            highlightGameObject.SetActive(true);
        }

        public void SetColor(Color newColor)
        {
            meshStandardColor = newColor;
            gameObject.GetComponent<Renderer>().material.color = newColor;
            colorPickerButtonBase.Color = newColor;
        }
    }
}