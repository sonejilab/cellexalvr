using UnityEngine;
using CellexalExtensions;
using UnityEditor;

/// <summary>
/// Represents a clickable text panel with some preset values. The preset values are set in the Unity Inspector.
/// </summary>
public class PresetClickableTextPanel : ClickablePanel
{
    public enum Operation { COLOR_BY, RESET_COLORS, SELECTION, HEATMAP }
    public DemoManager demoManager;
    public Operation operation;
    public Definitions.Measurement type;
    public string nameOfThing;
    public Material normalMaterial;
    public Material highlightedMaterial;
    public Material pressedMaterial;

    protected override void Start()
    {
        base.Start();
        SetMaterials(normalMaterial, highlightedMaterial, pressedMaterial);
    }

    public override void Click()
    {
        if (operation == Operation.COLOR_BY)
        {
            if (type == Definitions.Measurement.GENE)
            {
                referenceManager.cellManager.ColorGraphsByGene(nameOfThing);
                referenceManager.gameManager.InformColorGraphsByGene(nameOfThing);
            }
            else if (type == Definitions.Measurement.ATTRIBUTE)
            {
                referenceManager.cellManager.ColorByAttribute(nameOfThing, true);
                referenceManager.gameManager.InformColorByAttribute(nameOfThing, true);
            }
            else if (type == Definitions.Measurement.FACS)
            {
                referenceManager.cellManager.ColorByIndex(nameOfThing);
                referenceManager.gameManager.InformColorByIndex(nameOfThing);
            }
        }
        else if (operation == Operation.RESET_COLORS)
        {
            referenceManager.graphManager.ResetGraphsColor();
        }
        else if (operation == Operation.SELECTION)
        {
            demoManager.AdvanceSelection();
        }
        else if (operation == Operation.HEATMAP)
        {
            referenceManager.heatmapGenerator.CreateHeatmap();
        }
    }

}

#if UNITY_EDITOR
[CustomEditor(typeof(PresetClickableTextPanel))]
[CanEditMultipleObjects]
public class PresetClickableTextPanelEditor : UnityEditor.Editor
{

    private PresetClickableTextPanel instance;

    void OnEnable()
    {
        instance = (PresetClickableTextPanel)target;
    }

    public override void OnInspectorGUI()
    {
        instance.referenceManager = (ReferenceManager)EditorGUILayout.ObjectField("Reference Manager", instance.referenceManager, typeof(ReferenceManager), true);
        instance.demoManager = (DemoManager)EditorGUILayout.ObjectField("Demo Manager", instance.demoManager, typeof(DemoManager), true);
        instance.operation = (PresetClickableTextPanel.Operation)EditorGUILayout.EnumPopup("Operation", instance.operation);
        if (instance.operation == PresetClickableTextPanel.Operation.COLOR_BY)
        {
            instance.type = (Definitions.Measurement)EditorGUILayout.EnumPopup("Type", instance.type);
            string type = instance.type.ToString().ToLower();
            instance.nameOfThing = EditorGUILayout.TextField("Name of " + type, instance.nameOfThing);
        }
        instance.normalMaterial = (Material)EditorGUILayout.ObjectField("Normal material", instance.normalMaterial, typeof(Material), true);
        instance.highlightedMaterial = (Material)EditorGUILayout.ObjectField("Highlighted material", instance.highlightedMaterial, typeof(Material), true);
        instance.pressedMaterial = (Material)EditorGUILayout.ObjectField("Pressed material", instance.pressedMaterial, typeof(Material), true);

    }
}
#endif
