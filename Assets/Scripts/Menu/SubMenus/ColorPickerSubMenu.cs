using CellexalVR.General;
using CellexalVR.Menu.Buttons.ColorPicker;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace CellexalVR.Menu.SubMenus
{
    public class ColorPickerSubMenu : MenuWithTabs
    {
        public ColorPickerButton selectionColorButtonPrefab;
        public GameObject selectionColorButtonsParent;
        public GameObject addSelelectionColorButton;
        public ColorPickerButton geneExpressionHighButton;
        public ColorPickerButton geneExpressionMidButton;
        public ColorPickerButton geneExpressionLowButton;
        public ColorPickerButton networksPositiveHighButton;
        public ColorPickerButton networksPositiveLowButton;
        public ColorPickerButton networksNegativeHighButton;
        public ColorPickerButton networksNegativeLowButton;

        private Vector3 selectionColorButtonStartPos = new Vector3(-0.418f, 2.5f, 0.262f);
        private Vector3 selectionColorButtonRowInc = new Vector3(0.1f, 0f, 0f);
        private Vector3 selectionColorButtonColInc = new Vector3(0f, 0f, -0.1f);
        private Vector3 nextSelectionColorButtonPos;
        private int maxSelectionColorButtonPerRow = 9;
        private int maxSelectionColorButtonPerCol = 9;
        private List<ColorPickerButton> buttons = new List<ColorPickerButton>();
        public enum ConfigColor
        {
            NONE,
            HEATMAP_HIGH, HEATMAP_MID, HEATMAP_LOW,
            GRAPH_HIGH, GRAPH_MID, GRAPH_LOW, GRAPH_ZERO, GRAPH_DEFAULT,
            SELECTION,
            NETWORK_POSITIVE_HIGH, NETWORK_POSITIVE_LOW, NETWORK_NEGATIVE_HIGH, NETWORK_NEGATIVE_LOW,
            VELOCITY_HIGH, VELOCITY_LOW,
            SKYBOX_TINT
        }

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

            geneExpressionHighButton.SetColor(CellexalConfig.Config.GraphHighExpressionColor);
            geneExpressionMidButton.SetColor(CellexalConfig.Config.GraphMidExpressionColor);
            geneExpressionLowButton.SetColor(CellexalConfig.Config.HeatmapLowExpressionColor);
            networksPositiveHighButton.SetColor(CellexalConfig.Config.NetworkLineColorPositiveHigh);
            networksPositiveLowButton.SetColor(CellexalConfig.Config.NetworkLineColorPositiveLow);
            networksNegativeHighButton.SetColor(CellexalConfig.Config.NetworkLineColorNegativeHigh);
            networksNegativeLowButton.SetColor(CellexalConfig.Config.NetworkLineColorNegativeLow);
        }

        public void AddSelectionColorButton(Color col, int index)
        {
            if (index >= buttons.Count)
            {
                ColorPickerButton newButton = Instantiate(selectionColorButtonPrefab, selectionColorButtonsParent.transform);
                buttons.Add(newButton);
                newButton.colorPickerButtonBase.selectionToolColorIndex = index;
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
                buttons[index].colorPickerButtonBase.selectionToolColorIndex = index;
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
