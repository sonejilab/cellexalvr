using Assets.Scripts.Menu.ColorPicker;
using CellexalVR.General;
using CellexalVR.Menu.SubMenus;
using System.Collections.Generic;
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
        private int nextSelectionColorButtonRowIndex = 0;
        private int nextSelectionColorButtonColIndex = 0;
        private int maxSelectionColorButtonPerRow = 6;
        private int maxSelectionColorButtonPerCol = 6;
        private List<ColorPickerButton> buttons = new List<ColorPickerButton>();

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
            nextSelectionColorButtonPos = selectionColorButtonStartPos
                                         + selectionColorButtonRowInc * nextSelectionColorButtonRowIndex
                                         + selectionColorButtonColInc * nextSelectionColorButtonColIndex;
            ColorPickerButton newButton = Instantiate(selectionColorButtonPrefab, transform);
            buttons.Add(newButton);
            newButton.transform.localPosition = nextSelectionColorButtonPos;
            newButton.GetComponent<Renderer>().material.color = col;
            newButton.enabled = true;
            newButton.GetComponent<Renderer>().enabled = gameObject.GetComponent<Renderer>().enabled;
            newButton.GetComponent<Collider>().enabled = gameObject.GetComponent<Collider>().enabled;
            nextSelectionColorButtonRowIndex++;
            if (nextSelectionColorButtonRowIndex > maxSelectionColorButtonPerRow)
            {
                //start a new row
                nextSelectionColorButtonColIndex++;
                nextSelectionColorButtonRowIndex = 0;
                if (nextSelectionColorButtonColIndex > maxSelectionColorButtonPerCol)
                {
                    // start a new tab?
                }
            }

        }

        public void RemoveSelectionColorButton(ColorPickerButton button)
        {
            Destroy(button);
            buttons.Remove(button);
            for (int i = 0; i < buttons.Count; ++i)
            {
                buttons[i].transform.localPosition = selectionColorButtonStartPos
                                                     + selectionColorButtonRowInc * (i % maxSelectionColorButtonPerRow)
                                                     + selectionColorButtonColInc * ((i / maxSelectionColorButtonPerRow) % maxSelectionColorButtonPerCol);
            }
        }
    }
}
