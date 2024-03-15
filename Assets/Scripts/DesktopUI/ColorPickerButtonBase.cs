using CellexalVR.AnalysisLogic;
using CellexalVR.AnalysisObjects;
using CellexalVR.General;
using CellexalVR.Interaction;
using UnityEngine;

namespace CellexalVR.DesktopUI
{
    public class ColorPickerButtonBase : MonoBehaviour
    {
        public ReferenceManager referenceManager;

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
            VELOCITY_HIGH, VELOCITY_LOW,
            SKYBOX_TINT
        }
        public ConfigColor colorField;
        protected Color color;
        protected Material gradientMaterial;
        /// <summary>
        /// Updates the choice of color. Setting this property does not update the internal value. Make sure to call <see cref="FinalizeChoice"/> for that.
        /// </summary>
        /// <remarks>
        /// The reason for this split functionality is that the <see cref="FinalizeChoice"/> method calls some relatively performance heavy functions,
        /// while this property could be called once every frame to constantly update the visuals of the button.
        /// </remarks>
        public virtual Color Color
        {
            get { return color; }
            set
            {
                color = value;
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

        protected ColorPicker colorPicker;
        protected HeatmapGenerator heatmapGenerator;
        protected GraphGenerator graphGenerator;
        protected NetworkGenerator networkGenerator;
        protected SelectionToolCollider selectionToolCollider;
        protected SettingsMenu settingsMenu;

        protected void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();

                if (gradient)
                {
                    // Find out what type of component the gradient is rendering with
                    UnityEngine.UI.Image image = gradient.GetComponentInChildren<UnityEngine.UI.Image>(true);
                    if (image)
                    {
                        gradientMaterial = image.material;
                    }

                    MeshRenderer meshRenderer = gradient.GetComponentInChildren<MeshRenderer>(true);
                    if (meshRenderer)
                    {
                        gradientMaterial = meshRenderer.sharedMaterial;
                    }
                }
            }
        }

        protected void Start()
        {
            colorPicker = referenceManager.colorPicker;
            heatmapGenerator = referenceManager.heatmapGenerator;
            graphGenerator = referenceManager.graphGenerator;
            networkGenerator = referenceManager.networkGenerator;
            selectionToolCollider = referenceManager.selectionToolCollider;
            settingsMenu = referenceManager.settingsMenu;
        }

        /// <summary>
        /// Finalizes the color selection. Should be called when the user is done selection a color to set the internal value.
        /// </summary>
        public virtual void FinalizeChoice()
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
                case ConfigColor.SKYBOX_TINT:
                    CellexalConfig.Config.SkyboxTintColor = color;
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
                case ConfigColor.SKYBOX_TINT:
                    RenderSettings.skybox.SetColor("_Tint", color);
                    //RenderSettings.skybox.SetColor("_Tint", new Color(0, 0, 0));
                    break;
            }
        }

    }
}
