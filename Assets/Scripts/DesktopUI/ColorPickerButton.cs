using UnityEngine;

namespace CellexalVR.DesktopUI
{

    /// <summary>
    /// A button in the settings menu that picks a color using the <see cref="ColorPicker"/>.
    /// </summary>
    public class ColorPickerButton : ColorPickerButtonBase
    {
        public GameObject parentGroup;
        public UnityEngine.UI.Image image;

        private void Awake()
        {
            image = gameObject.GetComponent<UnityEngine.UI.Image>();
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