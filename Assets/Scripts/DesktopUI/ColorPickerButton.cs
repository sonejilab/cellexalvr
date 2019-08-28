using CellexalVR.AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Interaction;
using UnityEngine;

namespace CellexalVR.DesktopUI
{

    /// <summary>
    /// A button in the settings menu that picks a color using the <see cref="ColorPicker"/>.
    /// </summary>
    public class ColorPickerButton : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject parentGroup;
        public UnityEngine.UI.Image image;
        public enum GradientColor { COLOR1, COLOR2, COLOR3 }
        public GradientColor gradientColor;
        public GameObject gradient;
        public enum ConfigColor
        {
            NONE,
            HEATMAP_HIGH, HEATMAP_MID, HEATMAP_LOW,
            GRAPH_HIGH, GRAPH_MID, GRAPH_LOW, GRAPH_ZERO, GRAPH_DEFAULT,
            SELECTION,
            NETWORK_POSITIVE_HIGH, NETWORK_POSITIVE_LOW, NETWORK_NEGATIVE_HIGH, NETWORK_NEGATIVE_LOW,
            VELOCITY_HIGH, VELOCITY_LOW
        }
        public ConfigColor colorField;
        private Color color;
        private Material gradientMaterial;
        /// <summary>
        /// Updates the choice of color. Setting this property does not update the internal value. Make sure to call <see cref="FinalizeChoice"/> for that.
        /// </summary>
        /// <remarks>
        /// The reason for this split functionality is that the <see cref="FinalizeChoice"/> method calls some relatively performance heavy functions,
        /// while this property could be called once every frame to constantly update the visuals of the button.
        /// </remarks>
        public Color Color
        {
            get { return color; }
            set
            {
                color = value;
                image.color = value;
                if (gradientMaterial != null)
                {
                    if (gradientColor == GradientColor.COLOR1)
                    {
                        gradientMaterial.SetColor("_Color1", value);
                    }
                    else if (gradientColor == GradientColor.COLOR2)
                    {
                        gradientMaterial.SetColor("_Color2", value);
                    }
                    else if (gradientColor == GradientColor.COLOR3)
                    {
                        gradientMaterial.SetColor("_Color3", value);
                    }
                }
            }
        }
        [HideInInspector]
        public int selectionToolColorIndex;

        private ColorPicker colorPicker;
        private HeatmapGenerator heatmapGenerator;
        private GraphGenerator graphGenerator;
        private NetworkGenerator networkGenerator;
        private SelectionToolCollider selectionToolCollider;
        private SettingsMenu settingsMenu;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Awake()
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            image = gameObject.GetComponent<UnityEngine.UI.Image>();
            if (gradient != null)
            {
                gradientMaterial = gradient.GetComponent<UnityEngine.UI.Image>().material;
            }
        }

        private void Start()
        {
            colorPicker = referenceManager.colorPicker;
            heatmapGenerator = referenceManager.heatmapGenerator;
            graphGenerator = referenceManager.graphGenerator;
            networkGenerator = referenceManager.networkGenerator;
            selectionToolCollider = referenceManager.selectionToolCollider;
            settingsMenu = referenceManager.settingsMenu;
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

        /// <summary>
        /// Finilizes the choice of color and updates the appropriate values depending on what this button represents.
        /// </summary>
        public void FinalizeChoice()
        {
            // set the internal value
            switch (colorField)
            {
                case ConfigColor.HEATMAP_HIGH:
                    CellexalConfig.Config.HeatmapHighExpressionColor = color;
                    break;
                case ConfigColor.HEATMAP_MID:
                    CellexalConfig.Config.HeatmapMidExpressionColor = color;
                    break;
                case ConfigColor.HEATMAP_LOW:
                    CellexalConfig.Config.HeatmapLowExpressionColor = color;
                    break;
                case ConfigColor.GRAPH_HIGH:
                    CellexalConfig.Config.GraphHighExpressionColor = color;
                    break;
                case ConfigColor.GRAPH_MID:
                    CellexalConfig.Config.GraphMidExpressionColor = color;
                    break;
                case ConfigColor.GRAPH_LOW:
                    CellexalConfig.Config.GraphLowExpressionColor = color;
                    break;
                case ConfigColor.GRAPH_ZERO:
                    CellexalConfig.Config.GraphZeroExpressionColor = color;
                    break;
                case ConfigColor.GRAPH_DEFAULT:
                    CellexalConfig.Config.GraphDefaultColor = color;
                    break;
                case ConfigColor.NETWORK_POSITIVE_HIGH:
                    CellexalConfig.Config.NetworkLineColorPositiveHigh = color;
                    break;
                case ConfigColor.NETWORK_POSITIVE_LOW:
                    CellexalConfig.Config.NetworkLineColorPositiveLow = color;
                    break;
                case ConfigColor.NETWORK_NEGATIVE_HIGH:
                    CellexalConfig.Config.NetworkLineColorNegativeHigh = color;
                    break;
                case ConfigColor.NETWORK_NEGATIVE_LOW:
                    CellexalConfig.Config.NetworkLineColorNegativeLow = color;
                    break;
                case ConfigColor.SELECTION:
                    CellexalConfig.Config.SelectionToolColors[selectionToolColorIndex] = color;
                    referenceManager.graphGenerator.CreateShaderColors();
                    break;
                case ConfigColor.VELOCITY_HIGH:
                    CellexalConfig.Config.VelocityParticlesHighColor = color;
                    break;
                case ConfigColor.VELOCITY_LOW:
                    CellexalConfig.Config.VelocityParticlesLowColor = color;
                    break;
            }

            // call the appropriate method to update the necessary resources.
            switch (colorField)
            {
                case ConfigColor.HEATMAP_HIGH:
                case ConfigColor.HEATMAP_MID:
                case ConfigColor.HEATMAP_LOW:
                    heatmapGenerator.InitColors();
                    break;
                case ConfigColor.GRAPH_HIGH:
                case ConfigColor.GRAPH_MID:
                case ConfigColor.GRAPH_LOW:
                case ConfigColor.GRAPH_ZERO:
                case ConfigColor.GRAPH_DEFAULT:
                    graphGenerator.CreateShaderColors();
                    break;
                case ConfigColor.NETWORK_POSITIVE_HIGH:
                case ConfigColor.NETWORK_POSITIVE_LOW:
                case ConfigColor.NETWORK_NEGATIVE_HIGH:
                case ConfigColor.NETWORK_NEGATIVE_LOW:
                    networkGenerator.CreateLineMaterials();
                    break;
                case ConfigColor.SELECTION:
                    selectionToolCollider.UpdateColors();
                    break;
            }
            settingsMenu.ChangeMade();
        }
    }
}