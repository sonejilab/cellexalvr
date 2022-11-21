using Assets.Scripts.Menu.Buttons.ColorPicker;
using System.Collections;
using UnityEngine;

namespace Assets.Scripts.Menu.ColorPicker
{
    public class ColorPickerButton : CellexalVR.Menu.Buttons.CellexalButton
    {
        public ColorPickerPopout colorPicker;
        public GameObject highlightGameObject;
        [HideInInspector] public int index;

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
        }
    }
}