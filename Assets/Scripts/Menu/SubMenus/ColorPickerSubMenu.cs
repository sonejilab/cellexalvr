using Assets.Scripts.Menu.ColorPicker;
using CellexalVR.General;
using CellexalVR.Menu.SubMenus;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace Assets.Scripts.Menu.SubMenus
{
    public class ColorPickerSubMenu : SubMenu
    {
        public ColorPickerButton selectionColorButtonPrefab;
        public GameObject selectionColorButtonsParent;
        public GameObject addSelelectionColorButton;

        private Vector3 selectionColorButtonStartPos = new Vector3(-0.418f, 2.5f, 0.262f);
        private Vector3 selectionColorButtonRowInc = new Vector3(0.1f, 0f, 0f);
        private Vector3 selectionColorButtonColInc = new Vector3(0f, 0f, -0.1f);
        private Vector3 nextSelectionColorButtonPos;
        private int maxSelectionColorButtonPerRow = 9;
        private int maxSelectionColorButtonPerCol = 9;
        private List<ColorPickerButton> buttons = new List<ColorPickerButton>();

        private void Awake()
        {
            nextSelectionColorButtonPos = selectionColorButtonStartPos;
            CellexalEvents.ConfigLoaded.AddListener(OnConfigLoaded);
        }

        private void OnConfigLoaded()
        {
            for (int i = 0; i < CellexalConfig.Config.SelectionToolColors.Length; ++i)
            {
                Color col = CellexalConfig.Config.SelectionToolColors[i];
                AddSelectionColorButton(col, i);
            }
            if (buttons.Count > CellexalConfig.Config.SelectionToolColors.Length)
            {
                for (int i = CellexalConfig.Config.SelectionToolColors.Length; i < buttons.Count; ++i)
                {
                    Destroy(buttons[i].gameObject);
                }
                buttons.RemoveRange(CellexalConfig.Config.SelectionToolColors.Length, buttons.Count - CellexalConfig.Config.SelectionToolColors.Length);
            }
            addSelelectionColorButton.transform.localPosition = selectionColorButtonStartPos
                                                               + selectionColorButtonRowInc * (buttons.Count % maxSelectionColorButtonPerRow)
                                                               + selectionColorButtonColInc * (buttons.Count / maxSelectionColorButtonPerCol);
        }

        public void AddSelectionColorButton(Color col, int index)
        {
            if (index >= buttons.Count)
            {
                ColorPickerButton newButton = Instantiate(selectionColorButtonPrefab, selectionColorButtonsParent.transform);
                buttons.Add(newButton);
                newButton.index = index;
                nextSelectionColorButtonPos = selectionColorButtonStartPos
                                             + selectionColorButtonRowInc * (index % maxSelectionColorButtonPerRow)
                                             + selectionColorButtonColInc * (index / maxSelectionColorButtonPerCol);
                newButton.transform.localPosition = nextSelectionColorButtonPos;
                newButton.GetComponent<Renderer>().material.color = col;
                newButton.meshStandardColor = col;
                newButton.enabled = true;
                newButton.gameObject.SetActive(true);
                newButton.GetComponent<Renderer>().enabled = gameObject.GetComponent<Renderer>().enabled;
                newButton.GetComponent<Collider>().enabled = gameObject.GetComponent<Collider>().enabled;
                addSelelectionColorButton.transform.localPosition = selectionColorButtonStartPos
                                                                   + selectionColorButtonRowInc * ((index + 1) % maxSelectionColorButtonPerRow)
                                                                   + selectionColorButtonColInc * ((index + 1) / maxSelectionColorButtonPerCol);
            }
            else
            {
                buttons[index].GetComponent<Renderer>().material.color = col;
                buttons[index].meshStandardColor = col;
                buttons[index].index = index;
            }

        }

        public void RemoveSelectionColorButton(ColorPickerButton button)
        {
            button.highlightGameObject.SetActive(false);
            Color[] newColorArray = buttons.Where((ColorPickerButton b) => b != button)
                .Select((ColorPickerButton b) => b.meshStandardColor).ToArray();
            CellexalConfig.Config.SelectionToolColors = newColorArray;
            referenceManager.configManager.SaveConfigFile(referenceManager.configManager.currentProfileFullPath);
            CellexalEvents.ConfigLoaded.Invoke();
        }
    }
}
