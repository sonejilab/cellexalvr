using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Controls the settings menu and its components.
/// </summary>
public class SettingsMenu : MonoBehaviour
{

    public GameObject settingsMenuGameObject;
    public ReferenceManager referenceManager;
    public GameObject unsavedChangesPrompt;
    public GameObject resetAllSettingsPrompt;
    public HinterController hinterController;
    //[Header("Menu items")]
    [Header("Username")]
    public TMPro.TMP_InputField usernameInputField;
    public TMPro.TMP_Text usernameText;
    [Header("Heatmap")]
    public ColorPickerButton heatmapHighExpression;
    public ColorPickerButton heatmapMidExpression;
    public ColorPickerButton heatmapLowExpression;
    public Image heatmapGradient;
    public TMPro.TMP_InputField numberOfHeatmapColorsInputField;
    //public UnityEngine.UI.Dropdown heatmapAlgorithm;
    [Header("Graphs")]
    public ColorPickerButton graphHighExpression;
    public ColorPickerButton graphMidExpression;
    public ColorPickerButton graphLowExpression;
    public Image graphGradient;
    public TMPro.TMP_InputField numberOfGraphColorsInputField;
    public ColorPickerButton graphDefaultColor;
    public Toggle graphHightestExpressedMarker;
    [Header("Networks")]
    public Dropdown networkLineColoringMethod;
    public ColorPickerButton networkLinePositiveHigh;
    public ColorPickerButton networkLinePositiveLow;
    public Image networkPositiveGradient;
    public ColorPickerButton networkLineNegativeHigh;
    public ColorPickerButton networkLineNegativeLow;
    public Image networkNegativeGradient;
    public TMPro.TMP_InputField networkNumberOfNetworkColors;
    public TMPro.TMP_InputField networkLineWidth;
    [Header("Selection")]
    public GameObject selectionColorGroup;
    public GameObject selectionColorButtonPrefab;
    private List<ColorPickerButton> selectionColorButtons;
    public GameObject addSelectionColorButton;
    [Header("Visual")]
    public TMPro.TMP_Dropdown skyboxDropdown;

    public Material[] skyboxes;

    private ColorPicker colorPicker;
    private Config beforeChanges;
    private bool unsavedChanges;

    private void Awake()
    {
        CellexalEvents.ConfigLoaded.AddListener(SetValues);
        colorPicker = referenceManager.colorPicker;
        var skyboxOptions = new List<TMPro.TMP_Dropdown.OptionData>();
        foreach (Material mat in skyboxes)
        {
            skyboxOptions.Add(new TMPro.TMP_Dropdown.OptionData(mat.name));
        }
        skyboxDropdown.options = skyboxOptions;
        selectionColorButtons = new List<ColorPickerButton>();

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            bool newState = !settingsMenuGameObject.activeSelf;
            if (hinterController.hinter.activeSelf)
            {
                hinterController.HideHinter();
            }
            if (!unsavedChanges)
            {
                settingsMenuGameObject.SetActive(newState);
                colorPicker.gameObject.SetActive(newState);
            }
            if (!newState && unsavedChanges)
            {
                unsavedChangesPrompt.SetActive(true);
                colorPicker.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Sets the values of the buttons in the menu to accurately depict what the current internal values are.
    /// </summary>
    private void SetValues()
    {
        usernameText.text = "Current user: " + CellexalUser.Username;
        heatmapHighExpression.Color = CellexalConfig.Config.HeatmapHighExpressionColor;
        heatmapMidExpression.Color = CellexalConfig.Config.HeatmapMidExpressionColor;
        heatmapLowExpression.Color = CellexalConfig.Config.HeatmapLowExpressionColor;
        numberOfHeatmapColorsInputField.text = "" + CellexalConfig.Config.NumberOfHeatmapColors;
        graphHighExpression.Color = CellexalConfig.Config.GraphHighExpressionColor;
        graphMidExpression.Color = CellexalConfig.Config.GraphMidExpressionColor;
        graphLowExpression.Color = CellexalConfig.Config.GraphLowExpressionColor;
        numberOfGraphColorsInputField.text = "" + CellexalConfig.Config.GraphNumberOfExpressionColors;
        graphDefaultColor.Color = CellexalConfig.Config.GraphDefaultColor;
        graphHightestExpressedMarker.isOn = CellexalConfig.Config.GraphMostExpressedMarker;
        networkLineColoringMethod.value = CellexalConfig.Config.NetworkLineColoringMethod;
        networkLinePositiveHigh.Color = CellexalConfig.Config.NetworkLineColorPositiveHigh;
        networkLinePositiveLow.Color = CellexalConfig.Config.NetworkLineColorPositiveLow;
        networkLineNegativeHigh.Color = CellexalConfig.Config.NetworkLineColorNegativeHigh;
        networkLineNegativeLow.Color = CellexalConfig.Config.NetworkLineColorNegativeLow;
        networkNumberOfNetworkColors.text = "" + CellexalConfig.Config.NumberOfNetworkLineColors;
        networkLineWidth.text = "" + CellexalConfig.Config.NetworkLineWidth;

        for (int i = 0; i < CellexalConfig.Config.SelectionToolColors.Length; ++i)
        {
            if (i < selectionColorButtons.Count)
            {
                // there is already a button in the menu, change its color
                selectionColorButtons[i].Color = CellexalConfig.Config.SelectionToolColors[i];
            }
            else
            {
                // no more buttons, create one
                GameObject newButton = Instantiate(selectionColorButtonPrefab, selectionColorGroup.transform);
                newButton.SetActive(true);
                ColorPickerButton button = newButton.GetComponentInChildren<ColorPickerButton>();
                button.Color = CellexalConfig.Config.SelectionToolColors[i];
                button.selectionToolColorIndex = i;
                addSelectionColorButton.transform.SetAsLastSibling();
                selectionColorButtons.Add(button);
            }
        }

        SetNetworkColoringMethod();

        //LayoutRebuilder.MarkLayoutForRebuild((RectTransform)selectionColorGroup.transform);
        unsavedChanges = false;
        beforeChanges = new Config(CellexalConfig.Config);
    }

    private int TryParse(string s, int defaultValue)
    {
        if (s == null || s == "")
        {
            return defaultValue;
        }
        return int.Parse(s);
    }

    private float TryParse(string s, float defaultValue)
    {
        if (s == null || s == "")
        {
            return defaultValue;
        }
        return float.Parse(s);
    }

    public void SetUser()
    {
        unsavedChanges = true;
        string name = usernameInputField.text;
        CellexalUser.Username = name;
        usernameText.text = "Current user: " + name;
    }

    public void SetNumberOfHeatmapColors()
    {
        unsavedChanges = true;
        int nColors = TryParse(numberOfHeatmapColorsInputField.text, 3);
        if (nColors < 3)
        {
            CellexalLog.Log("WARNING: Number of heatmap expression colors must be at least 3. Defaulting to 3.");
            nColors = 3;
        }
        numberOfHeatmapColorsInputField.text = "" + nColors;
        CellexalConfig.Config.NumberOfHeatmapColors = nColors;
        referenceManager.heatmapGenerator.InitColors();
        heatmapGradient.material.SetInt("_NColors", nColors);
    }

    public void SetNumberOfGraphColors()
    {
        unsavedChanges = true;
        int nColors = TryParse(numberOfGraphColorsInputField.text, 3);
        if (nColors < 3)
        {
            CellexalLog.Log("WARNING: Number of graph expression colors must be at least 3. Defaulting to 3.");
            nColors = 3;
        }
        numberOfGraphColorsInputField.text = "" + nColors;
        CellexalConfig.Config.GraphNumberOfExpressionColors = nColors;
        referenceManager.combinedGraphGenerator.CreateShaderColors();
        graphGradient.material.SetInt("_NColors", nColors);
    }

    public void SetNetworkColoringMethod()
    {
        unsavedChanges = true;
        int newMethod = networkLineColoringMethod.value;
        CellexalConfig.Config.NetworkLineColoringMethod = newMethod;

        bool active = newMethod == 0;
        networkLinePositiveHigh.parentGroup.SetActive(active);
        networkLinePositiveLow.parentGroup.SetActive(active);
        networkLineNegativeHigh.parentGroup.SetActive(active);
        networkLineNegativeLow.parentGroup.SetActive(active);
        SetNumberOfNetworkColors();
    }

    public void SetNumberOfNetworkColors()
    {
        unsavedChanges = true;
        int nColors = TryParse(networkNumberOfNetworkColors.text, 1);
        if (CellexalConfig.Config.NetworkLineColoringMethod == 0 &&
            nColors < 4)
        {
            CellexalLog.Log("WARNING: Number of network line colors must be at least 4 when coloring method is set to by correlation. Defaulting to 4.");
            nColors = 4;
        }
        else if (CellexalConfig.Config.NetworkLineColoringMethod == 1 &&
            nColors < 1)
        {
            CellexalLog.Log("WARNING: Number of network line colors must be at least 1 when coloring method is set to random. Defaulting to 1.");
            nColors = 1;

        }
        networkNumberOfNetworkColors.text = "" + nColors;
        CellexalConfig.Config.NumberOfNetworkLineColors = nColors;
        referenceManager.networkGenerator.CreateLineMaterials();
        int halfColors = nColors / 2;
        networkPositiveGradient.material.SetInt("_NColors", halfColors);
        networkNegativeGradient.material.SetInt("_NColors", nColors - halfColors);
    }

    public void SetNetworkLineWidth()
    {
        unsavedChanges = true;
        float newValue = TryParse(networkLineWidth.text, 0.001f);
        if (newValue <= 0)
        {
            CellexalLog.Log("WARNING: Network line width may not be a negative value. Defaulting to 0.001.");
            newValue = 0.001f;
            networkLineWidth.text = "" + newValue;
        }
        CellexalConfig.Config.NetworkLineWidth = newValue;
    }

    public void SetSkyBox()
    {
        unsavedChanges = true;
        int selected = skyboxDropdown.value;
        RenderSettings.skybox = skyboxes[selected];
    }

    public void AddSelectionColor()
    {
        unsavedChanges = true;
        GameObject newButton = Instantiate(selectionColorButtonPrefab, selectionColorGroup.transform);
        newButton.SetActive(true);
        addSelectionColorButton.transform.SetAsLastSibling();
        selectionColorButtons.Add(newButton.GetComponentInChildren<ColorPickerButton>());
        UpdateSelectionToolColors();
    }

    public void RemoveSelectionColor(GameObject button)
    {
        unsavedChanges = true;
        selectionColorButtons.Remove(button.GetComponentInChildren<ColorPickerButton>());
        Destroy(button);
        UpdateSelectionToolColors();
    }

    public void UpdateSelectionToolColors()
    {
        Color[] colors = new Color[selectionColorButtons.Count];
        for (int i = 0; i < selectionColorButtons.Count; ++i)
        {
            selectionColorButtons[i].selectionToolColorIndex = i;
            colors[i] = selectionColorButtons[i].Color;
        }
        CellexalConfig.Config.SelectionToolColors = colors;
        referenceManager.selectionToolHandler.UpdateColors();
    }

    public void SetGraphHighestExpressionMarker(bool active)
    {
        CellexalConfig.Config.GraphMostExpressedMarker = active;
    }

    public void ChangeMade()
    {
        unsavedChanges = true;
    }

    public void SaveAndClose()
    {
        CellexalLog.Log("Saved changes made in the settings menu.");
        unsavedChanges = false;
        referenceManager.configManager.SaveConfigFile();
        unsavedChangesPrompt.SetActive(false);
        settingsMenuGameObject.SetActive(false);
        colorPicker.gameObject.SetActive(false);
    }


    public void ShowResetAllSettings(bool show)
    {
        resetAllSettingsPrompt.SetActive(show);
    }

    public void ResetSettingsToFile()
    {
        referenceManager.configManager.ResetToDefault();
        resetAllSettingsPrompt.SetActive(false);
    }

    public void ResetSettingsToBefore()
    {
        CellexalLog.Log("Reset changes made in settings menu to what they were before.");
        CellexalConfig.Config = new Config(beforeChanges);
        SetValues();
        unsavedChangesPrompt.SetActive(false);
    }

    /// <summary>
    /// Quits the program.
    /// </summary>
    public void Quit()
    {
        CellexalLog.Log("Quit button pressed");
        CellexalLog.LogBacklog();
        // Application.Quit() does not work in the unity editor, only in standalone builds.
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

}
