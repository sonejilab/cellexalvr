using Assets.Scripts.Menu.ColorPicker;
using CellexalVR.General;
using CellexalVR.Menu.SubMenus;
using UnityEngine;

namespace Assets.Scripts.Menu.SubMenus
{
    public class ColorPickerSubMenu : SubMenu
    {
        public ColorPickerButton selectionColorButtonPrefab;

        private Vector3 selectionColorButtonStartPos = new Vector3(-0.418f, 2.5f, 0.262f);
        private Vector3 selectionColorButtonRowInc = new Vector3(0.1f, 0f, 0f);
        private Vector3 selectionColorButtonColInc = new Vector3(0f, 0.1f, 0f);
        private Vector3 nextSelectionColorButtonPos;

        private void Awake()
        {
            nextSelectionColorButtonPos = selectionColorButtonStartPos;
            CellexalEvents.ConfigLoaded.AddListener(OnConfigLoaded);
        }

        private void OnConfigLoaded()
        {
            foreach (Color col in CellexalConfig.Config.SelectionToolColors)
            {
                AddSelectionColorButton(col);
            }
        }

        public void AddSelectionColorButton(Color col)
        {

        }

        public void RemoveSelectionColorButton(CellexalVR.Menu.Buttons.CellexalButton button)
        {

        }
    }
}
