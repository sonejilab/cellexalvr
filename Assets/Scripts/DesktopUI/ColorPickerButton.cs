using UnityEngine;
using UnityEngine.UI;

namespace CellexalVR.DesktopUI
{

    /// <summary>
    /// A button in the settings menu that picks a color using the <see cref="ColorPicker"/>.
    /// </summary>
    public class ColorPickerButton : ColorPickerButtonBase
    {
        public GameObject parentGroup;
        public Image image;
        public override Color Color
        {
            get => base.Color;
            set
            {
                base.Color = value;
                image.color = value;
            }
        }

        private void Awake()
        {
            image = gameObject.GetComponent<Image>();
        }


        /// <summary>
        /// Summons the color picker to this buttons postition.
        /// </summary>
        public void SummonColorPicker()
        {
            if (!colorPicker.gameObject.activeSelf)
            {
                colorPicker.gameObject.SetActive(true);
            }
            colorPicker.activeButton = this;
            colorPicker.MoveToDesiredPosition(new Vector3(550, transform.position.y, 0));
            colorPicker.SetColor(image.color);
        }

        public override void FinalizeChoice()
        {
            base.FinalizeChoice();
            settingsMenu.unsavedChanges = true;
        }
    }
}