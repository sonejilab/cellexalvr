using System;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// A button in the settings menu that picks a color using the <see cref="ColorPicker"/>.
/// </summary>
public class ColorPickerButton : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public UnityEngine.UI.Image image;
    public GameObject parentGroup;

    public enum ConfigColor
    {
        NONE,
        HEATMAP_HIGH, HEATMAP_MID, HEATMAP_LOW,
        GRAPH_HIGH, GRAPH_MID, GRAPH_LOW,
        SELECTION,
        DEFAULT,
        NETWORK_POSITIVE_HIGH, NETWORK_POSITIVE_LOW, NETWORK_NEGATIVE_HIGH, NETWORK_NEGATIVE_LOW
    }
    public ConfigColor colorField;
    public int selectionToolColorIndex;

    private ColorPicker colorPicker;
    private HeatmapGenerator heatmapGenerator;
    private CombinedGraphGenerator combinedGraphGenerator;
    private NetworkGenerator networkGenerator;
    private SelectionToolHandler selectionToolHandler;

    private void Start()
    {
        image = GetComponent<UnityEngine.UI.Image>();
        colorPicker = referenceManager.colorPicker;
        heatmapGenerator = referenceManager.heatmapGenerator;
        combinedGraphGenerator = referenceManager.combinedGraphGenerator;
        networkGenerator = referenceManager.networkGenerator;
        selectionToolHandler = referenceManager.selectionToolHandler;
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
    public void ChooseColor()
    {
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
            case ConfigColor.DEFAULT:
                combinedGraphGenerator.CreateShaderColors();
                break;
            case ConfigColor.NETWORK_POSITIVE_HIGH:
            case ConfigColor.NETWORK_POSITIVE_LOW:
            case ConfigColor.NETWORK_NEGATIVE_HIGH:
            case ConfigColor.NETWORK_NEGATIVE_LOW:
                networkGenerator.CreateLineMaterials();
                break;
            case ConfigColor.SELECTION:
                selectionToolHandler.UpdateColors();
                break;

        }
    }

    /// <summary>
    /// Updates the choice of color.
    /// </summary>
    /// <param name="color">The new color to choose.</param>
    public void SetColor(Color color)
    {
        image.color = color;

        switch (colorField)
        {
            case ConfigColor.HEATMAP_HIGH:
                CellexalConfig.HeatmapHighExpressionColor = color;
                break;
            case ConfigColor.HEATMAP_MID:
                CellexalConfig.HeatmapMidExpressionColor = color;
                break;
            case ConfigColor.HEATMAP_LOW:
                CellexalConfig.HeatmapLowExpressionColor = color;
                break;
            case ConfigColor.GRAPH_HIGH:
                CellexalConfig.GraphHighExpressionColor = color;
                break;
            case ConfigColor.GRAPH_MID:
                CellexalConfig.GraphMidExpressionColor = color;
                break;
            case ConfigColor.GRAPH_LOW:
                CellexalConfig.GraphLowExpressionColor = color;
                break;
            case ConfigColor.DEFAULT:
                CellexalConfig.GraphDefaultColor = color;
                break;
            case ConfigColor.NETWORK_POSITIVE_HIGH:
                CellexalConfig.NetworkLineColorPositiveHigh = color;
                break;
            case ConfigColor.NETWORK_POSITIVE_LOW:
                CellexalConfig.NetworkLineColorPositiveLow = color;
                break;
            case ConfigColor.NETWORK_NEGATIVE_HIGH:
                CellexalConfig.NetworkLineColorNegativeHigh = color;
                break;
            case ConfigColor.NETWORK_NEGATIVE_LOW:
                CellexalConfig.NetworkLineColorNegativeLow = color;
                break;
            case ConfigColor.SELECTION:
                if (CellexalConfig.SelectionToolColors.Length <= selectionToolColorIndex)
                {
                    Color[] newArray = new Color[selectionToolColorIndex + 1];
                    Array.Copy(CellexalConfig.SelectionToolColors, newArray, selectionToolColorIndex + 1);
                }
                CellexalConfig.SelectionToolColors[selectionToolColorIndex] = color;
                break;
        }
    }

}
