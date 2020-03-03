using UnityEngine;
using CellexalVR.General;

namespace CellexalVR.DesktopUI
{

    public class ColormapManager : MonoBehaviour
    {
        public TMPro.TMP_InputField nrGroupsInput;

        private SettingsMenu settingsMenu;
        // Use this for initialization
        private void Start()
        {
            settingsMenu = GetComponent<SettingsMenu>();
        }

        public void GenerateRandomColors(int n = 20)
        {
            DoGenerateRandomColors(n);
            settingsMenu.referenceManager.multiuserMessageSender.SendMessageGenerateRandomColors(n);

        }

        public void DoGenerateRandomColors(int n, bool multiuserUpdate = false)
        {
            if (nrGroupsInput.text != "" && !multiuserUpdate)
            {
                n = int.Parse(nrGroupsInput.text);
            }
            if (n > 254)
            {
                n = 254;
            }
            foreach (ColorPickerButton button in settingsMenu.selectionColorButtons)//for (int i = 0; i < settingsMenu.selectionColorButtons.Count; i++)
            {
                Destroy(button.transform.parent.gameObject);
            }
            settingsMenu.selectionColorButtons.Clear();
            for (int j = 0; j < n; j++)
            {
                settingsMenu.AddSelectionColor(false);
            }
            settingsMenu.UpdateSelectionToolColors();
        }

        public void GenerateRainbowColors(int n = 20)
        {
            DoGenerateRainbowColors(n);
            settingsMenu.referenceManager.multiuserMessageSender.SendMessageGenerateRainbowColors(n);
        }

        public void DoGenerateRainbowColors(int n, bool multiuserUpdate = false)
        {
            if (nrGroupsInput.text != "" && !multiuserUpdate)
            {
                n = int.Parse(nrGroupsInput.text);
            }
            if (n > 254)
            {
                n = 254;
            }

            foreach (ColorPickerButton button in settingsMenu.selectionColorButtons)//for (int i = 0; i < settingsMenu.selectionColorButtons.Count; i++)
            {
                Destroy(button.transform.parent.gameObject);
            }
            settingsMenu.selectionColorButtons.Clear();
            for (float j = 0f; j < 1.0f; j += 1.0f / (float)n)
            {
                Color col = Color.HSVToRGB(j, 0.8f, 0.8f);
                settingsMenu.AddSelectionColor(col, false);
            }
            settingsMenu.UpdateSelectionToolColors();
        }
        public void SetHeatmapColormap()
        {
            settingsMenu.unsavedChanges = true;
            int val = settingsMenu.heatmapColormapDropdown.value;
            string colormap = settingsMenu.heatmapColormapDropdown.options[val].text;
            switch (colormap)
            {
                case "Viridis":
                    CellexalConfig.Config.HeatmapHighExpressionColor = new Color(0.993248f, 0.906157f, 0.143936f);
                    CellexalConfig.Config.HeatmapMidExpressionColor = new Color(0.127568f, 0.566949f, 0.550556f);
                    CellexalConfig.Config.HeatmapLowExpressionColor = new Color(0.267004f, 0.004874f, 0.329415f);
                    break;
                case "Jet":
                    CellexalConfig.Config.HeatmapHighExpressionColor = new Color(0.7f, 0.0f, 0.0f);
                    CellexalConfig.Config.HeatmapMidExpressionColor = new Color(0.9f, 0.9f, 0.1f);
                    CellexalConfig.Config.HeatmapLowExpressionColor = new Color(0.0f, 0.0f, 0.7f);

                    break;
                case "Plasma":
                    CellexalConfig.Config.HeatmapHighExpressionColor = new Color(0.940015f, 0.975158f, 0.131326f);
                    CellexalConfig.Config.HeatmapMidExpressionColor = new Color(0.798216f, 0.280197f, 0.469538f);
                    CellexalConfig.Config.HeatmapLowExpressionColor = new Color(0.050383f, 0.029803f, 0.527975f);
                    break;
            }
            settingsMenu.referenceManager.heatmapGenerator.InitColors();
            settingsMenu.heatmapHighExpression.Color = CellexalConfig.Config.HeatmapHighExpressionColor;
            settingsMenu.heatmapMidExpression.Color = CellexalConfig.Config.HeatmapMidExpressionColor;
            settingsMenu.heatmapLowExpression.Color = CellexalConfig.Config.HeatmapLowExpressionColor;
        }
        
        public void SetGraphColormap()
        {
            settingsMenu.unsavedChanges = true;
            int val = settingsMenu.graphColormapDropdown.value;
            string colormap = settingsMenu.graphColormapDropdown.options[val].text;
            switch (colormap)
            {
                case "Viridis":
                    CellexalConfig.Config.GraphHighExpressionColor = new Color(0.993248f, 0.906157f, 0.143936f);
                    CellexalConfig.Config.GraphMidExpressionColor = new Color(0.127568f, 0.566949f, 0.550556f);
                    CellexalConfig.Config.GraphLowExpressionColor = new Color(0.267004f, 0.004874f, 0.329415f);
                    break;
                case "Jet":
                    CellexalConfig.Config.GraphHighExpressionColor = new Color(0.9f, 0.0f, 0.0f);
                    CellexalConfig.Config.GraphMidExpressionColor = new Color(0.9f, 0.9f, 0.1f);
                    CellexalConfig.Config.GraphLowExpressionColor = new Color(0.0f, 0.0f, 0.9f);

                    break;
                case "Plasma":
                    CellexalConfig.Config.GraphHighExpressionColor = new Color(0.940015f, 0.975158f, 0.131326f);
                    CellexalConfig.Config.GraphMidExpressionColor = new Color(0.798216f, 0.280197f, 0.469538f);
                    CellexalConfig.Config.GraphLowExpressionColor = new Color(0.050383f, 0.029803f, 0.527975f);
                    break;
            }
            settingsMenu.referenceManager.graphGenerator.CreateShaderColors();
            settingsMenu.graphHighExpression.Color = CellexalConfig.Config.GraphHighExpressionColor;
            settingsMenu.graphMidExpression.Color = CellexalConfig.Config.GraphMidExpressionColor;
            settingsMenu.graphLowExpression.Color = CellexalConfig.Config.GraphLowExpressionColor;
        }


    }
}
