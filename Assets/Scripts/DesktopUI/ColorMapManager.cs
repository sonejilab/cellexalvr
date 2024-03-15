using System.Collections.Generic;
using UnityEngine;
using CellexalVR.General;

namespace CellexalVR.DesktopUI
{
    public class ColorMapManager : MonoBehaviour
    {
        public TMPro.TMP_InputField nrGroupsInput;

        /// <summary>
        /// A pre-defined list of colors taken from matplotlibs color map tab20.
        /// https://matplotlib.org/stable/gallery/color/colormap_reference.html
        /// </summary>
        public static List<Color> tab20 = new List<Color>
        {
            new Color(0.12157f, 0.46666f, 0.70588f, 1.0f),
            new Color(0.68235f, 0.78039f, 0.90980f, 1.0f),
            new Color(1.0f, 0.49803f, 0.05490f, 1.0f),
            new Color(1.0f, 0.73333f, 0.47058f, 1.0f),
            new Color(0.17254f, 0.62745f, 0.17254f, 1.0f),
            new Color(0.59607f, 0.87450f, 0.54117f, 1.0f),
            new Color(0.83921f, 0.15294f, 0.15686f, 1.0f),
            new Color(1.0f, 0.59607f, 0.58823f, 1.0f),
            new Color(0.58039f, 0.403921f, 0.74117f, 1.0f),
            new Color(0.77254f, 0.690196f, 0.83529f, 1.0f),
            new Color(0.54901f, 0.337254f, 0.29411f, 1.0f),
            new Color(0.76862f, 0.611764f, 0.58039f, 1.0f),
            new Color(0.89019f, 0.466666f, 0.76078f, 1.0f),
            new Color(0.96862f, 0.713725f, 0.82352f, 1.0f),
            new Color(0.49803f, 0.498039f, 0.49803f, 1.0f),
            new Color(0.78039f, 0.780392f, 0.78039f, 1.0f),
            new Color(0.73725f, 0.741176f, 0.13333f, 1.0f),
            new Color(0.85882f, 0.858823f, 0.55294f, 1.0f),
            new Color(0.09019f, 0.745098f, 0.81176f, 1.0f),
            new Color(0.61960f, 0.854901f, 0.89803f, 1.0f)
        };

        private SettingsMenu settingsMenu;

        private Dictionary<string, List<Color>> colorMaps = new Dictionary<string, List<Color>>();

        // Use this for initialization
        private void Start()
        {
            settingsMenu = GetComponent<SettingsMenu>();
            colorMaps["tab20"] = tab20;
        }

        public void SetColorMap()
        {
            settingsMenu.unsavedChanges = true;
            int val = settingsMenu.selectionColorMapDropdown.value;
            string name = settingsMenu.selectionColorMapDropdown.options[val].text;
            switch (name)
            {
                case "tab20":
                    List<Color> colorMap = colorMaps[name];
                    foreach (ColorPickerButton button in settingsMenu.selectionColorButtons) //for (int i = 0; i < settingsMenu.selectionColorButtons.Count; i++)
                    {
                        Destroy(button.transform.parent.gameObject);
                    }

                    settingsMenu.selectionColorButtons.Clear();
                    foreach (Color col in colorMap)
                    {
                        settingsMenu.AddSelectionColor(col, false);
                    }

                    settingsMenu.UpdateSelectionToolColors();
                    break;
            }
            settingsMenu.referenceManager.graphGenerator.CreateShaderColors();
        }

        /// <summary>
        /// Generates random colors to use as selection colors.
        /// </summary>
        /// <param name="n">Optional, the numbers of colors to generate. If left out, the number in the desktop UI field will be used.</param>
        public void GenerateRandomColors(int n = -1)
        {
            if (n == -1)
            {
                if (nrGroupsInput.text == "")
                {
                    n = 20;
                }
                else
                {
                    n = int.Parse(nrGroupsInput.text);
                }
            }
            DoGenerateRandomColors(n);
            settingsMenu.referenceManager.multiuserMessageSender.SendMessageGenerateRandomColors(n);
        }

        /// <summary>
        /// Generates random colors to use as selection colors.
        /// </summary>
        /// <param name="n">The number of colors to generate.</param>
        public void DoGenerateRandomColors(int n)
        {
            if (n > 254)
            {
                n = 254;
            }

            foreach (ColorPickerButton button in settingsMenu.selectionColorButtons) //for (int i = 0; i < settingsMenu.selectionColorButtons.Count; i++)
            {
                Destroy(button.transform.parent.gameObject);
            }

            settingsMenu.selectionColorButtons.Clear();
            for (int j = 0;
                j < n;
                j++)
            {
                settingsMenu.AddSelectionColor(false);
            }

            settingsMenu.UpdateSelectionToolColors();
        }

        /// <summary>
        /// Generates a rainbow pattern of colors to use as selection colors.
        /// </summary>
        /// <param name="n">Optional, the numbers of colors to generate. If left out, the number in the desktop UI field will be used.</param>
        public void GenerateRainbowColors(int n = -1)
        {
            if (n == -1)
            {
                if (nrGroupsInput.text == "")
                {
                    n = 20;
                }
                else
                {
                    n = int.Parse(nrGroupsInput.text);
                }
            }
            DoGenerateRainbowColors(n);
            settingsMenu.referenceManager.multiuserMessageSender.SendMessageGenerateRainbowColors(n);
        }

        /// <summary>
        /// Generates a rainbow pattern of colors to use as selection colors.
        /// </summary>
        /// <param name="n">The number of colors to generate.</param>
        public void DoGenerateRainbowColors(int n)
        {
            if (n > 254)
            {
                n = 254;
            }

            foreach (ColorPickerButton button in settingsMenu.selectionColorButtons) //for (int i = 0; i < settingsMenu.selectionColorButtons.Count; i++)
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

        /// <summary>
        /// Sets the heatmaps' expression colors.
        /// The colors to use are set through the desktop UI.
        /// </summary>
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

        /// <summary>
        /// Sets the graphs' gene expression colors to some pre-defined color sets.
        /// The colors to use are set through the desktop UI.
        /// </summary>
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