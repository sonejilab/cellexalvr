using UnityEditor;
using UnityEngine;
/// <summary>
/// A custom inspector for all buttons to help show only fields that are used.
/// </summary>

[CustomEditor(typeof(CellexalButton), editorForChildClasses: true)]
[CanEditMultipleObjects]
public class CellexalButtonEditor : Editor
{


    public string[] buttonTypeOptions = new string[] { "Mesh", "Sprite" };

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        var buttonScript = target as CellexalButton;
        buttonScript.popupChoice = EditorGUILayout.Popup("Button Type", buttonScript.popupChoice, buttonTypeOptions, EditorStyles.popup);
        if (buttonScript.popupChoice == 0)
        {
            //EditorGUILayout.PrefixLabel("Mesh options");
            buttonScript.meshStandardColor = EditorGUILayout.ColorField("Standard Color", buttonScript.meshStandardColor);
            buttonScript.meshHighlightColor = EditorGUILayout.ColorField("Highlighted Color", buttonScript.meshHighlightColor);
            buttonScript.meshDeactivatedColor = EditorGUILayout.ColorField("Deactivated Color", buttonScript.meshDeactivatedColor);

        }
        else if (buttonScript.popupChoice == 1)
        {
            //EditorGUILayout.PrefixLabel("Sprite options");
            buttonScript.standardTexture = (UnityEngine.Sprite)EditorGUILayout.ObjectField("Standard texture", buttonScript.standardTexture, typeof(UnityEngine.Sprite), true);
            buttonScript.highlightedTexture = (UnityEngine.Sprite)EditorGUILayout.ObjectField("Highlighted texture", buttonScript.highlightedTexture, typeof(UnityEngine.Sprite), true);
            buttonScript.deactivatedTexture = (UnityEngine.Sprite)EditorGUILayout.ObjectField("Deactivated texture", buttonScript.deactivatedTexture, typeof(UnityEngine.Sprite), true);
        }
        EditorUtility.SetDirty(buttonScript);
        base.OnInspectorGUI();

        serializedObject.ApplyModifiedProperties();
    }

}

