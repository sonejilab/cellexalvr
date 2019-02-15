using UnityEngine;
using System.Collections;
using UnityEngine.Events;

/// <summary>
/// Controls the settings menu and its components.
/// </summary>
public class SettingsMenu : MonoBehaviour
{

    public GameObject settingsMenuGameObject;
    public ReferenceManager referenceManager;
    [Header("Menu items")]
    // username
    public TMPro.TMP_InputField usernameInputField;
    public TMPro.TMP_Text usernameText;
    // heatmap
    public ColorPickerButton heatmapHighExpression;
    public ColorPickerButton heatmapMidExpression;
    public ColorPickerButton heatmapLowExpression;
    public TMPro.TMP_InputField numberOfHeatmapColorsInputField;
    //public UnityEngine.UI.Dropdown heatmapAlgorithm;
    // graphs
    public ColorPickerButton graphHighExpression;
    public ColorPickerButton graphMidExpression;
    public ColorPickerButton graphLowExpression;
    public TMPro.TMP_InputField numberOfGraphColorsInputField;
    public ColorPickerButton graphDefaultColor;
    //networks
    public UnityEngine.UI.Dropdown networkLineColoringMethod;
    public ColorPickerButton networkLinePositiveHigh;
    public ColorPickerButton networkLinePositiveLow;
    public ColorPickerButton networkLineNegativeHigh;
    public ColorPickerButton networkLineNegativeLow;
    // visual
    public TMPro.TMP_Dropdown skyboxDropdown;

    public Material[] skyboxes;

    private ColorPicker colorPicker;


    //public string[] configFields;
    private Object source;

    private void Awake()
    {
        CellexalEvents.ConfigLoaded.AddListener(SetValues);
        colorPicker = referenceManager.colorPicker;
        var skyboxOptions = new System.Collections.Generic.List<TMPro.TMP_Dropdown.OptionData>();
        foreach (Material mat in skyboxes)
        {
            skyboxOptions.Add(new TMPro.TMP_Dropdown.OptionData(mat.name));
        }
        skyboxDropdown.options = skyboxOptions;
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool newState = !settingsMenuGameObject.activeSelf;
            settingsMenuGameObject.SetActive(newState);
            colorPicker.gameObject.SetActive(newState);
        }
    }

    /// <summary>
    /// Sets the values of the buttons in the menu to accurately depict what the current internal values are.
    /// </summary>
    private void SetValues()
    {
        usernameText.text = "Current user: " + CellexalUser.Username;
        heatmapHighExpression.SetColor(CellexalConfig.HeatmapHighExpressionColor);
        heatmapMidExpression.SetColor(CellexalConfig.HeatmapMidExpressionColor);
        heatmapLowExpression.SetColor(CellexalConfig.HeatmapLowExpressionColor);
        numberOfHeatmapColorsInputField.text = "" + CellexalConfig.NumberOfHeatmapColors;
        graphHighExpression.SetColor(CellexalConfig.GraphHighExpressionColor);
        graphMidExpression.SetColor(CellexalConfig.GraphMidExpressionColor);
        graphLowExpression.SetColor(CellexalConfig.GraphLowExpressionColor);
        numberOfGraphColorsInputField.text = "" + CellexalConfig.GraphNumberOfExpressionColors;
        graphDefaultColor.SetColor(CellexalConfig.GraphDefaultColor);
        networkLineColoringMethod.value = CellexalConfig.NetworkLineColoringMethod;
        UpdateNetworkLineColorButtonsActive();
        networkLinePositiveHigh.SetColor(CellexalConfig.NetworkLineColorPositiveHigh);
        networkLinePositiveLow.SetColor(CellexalConfig.NetworkLineColorPositiveLow);
        networkLineNegativeHigh.SetColor(CellexalConfig.NetworkLineColorNegativeHigh);
        networkLineNegativeLow.SetColor(CellexalConfig.NetworkLineColorNegativeLow);
    }

    public void SetUser()
    {
        string name = usernameInputField.text;
        CellexalUser.Username = name;
        usernameText.text = "Current user: " + name;
    }

    public void SetNumberOfHeatmapColors()
    {
        int nColors = int.Parse(numberOfHeatmapColorsInputField.text);
        CellexalConfig.NumberOfHeatmapColors = nColors;
        referenceManager.heatmapGenerator.InitColors();
    }

    public void SetNumberOfGraphColors()
    {
        int nColors = int.Parse(numberOfGraphColorsInputField.text);
        CellexalConfig.GraphNumberOfExpressionColors = nColors;
        referenceManager.combinedGraphGenerator.CreateShaderColors();
    }

    public void UpdateNetworkLineColorButtonsActive()
    {
        bool active = networkLineColoringMethod.value == 0;

        networkLinePositiveHigh.parentGroup.SetActive(active);
        networkLinePositiveLow.parentGroup.SetActive(active);
        networkLineNegativeHigh.parentGroup.SetActive(active);
        networkLineNegativeLow.parentGroup.SetActive(active);
    }

    public void SetSkyBox()
    {
        int selected = skyboxDropdown.value;
        RenderSettings.skybox = skyboxes[selected];
    }
}
