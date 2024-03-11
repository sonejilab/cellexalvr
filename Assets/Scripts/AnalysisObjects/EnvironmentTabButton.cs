using CellexalVR.Menu.Buttons;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CellexalVR.AnalysisObjects
{
    public class EnvironmentTabButton : CellexalButton
    {

        public EnvironmentMenuWithTabs parentMenu;
        public EnvironmentTab parentTab;

        // Drawn in EnvironmentTabButtonEditor.DrawMeshOrSpriteFields()
        [HideInInspector]
        public Color meshActivatedColor;
        [HideInInspector]
        public Color meshActivatedHighlightedColor;
        [HideInInspector]
        public Sprite meshActivatedTexture;
        [HideInInspector]
        public Sprite meshActivatedHighlightedTexture;

        protected override string Description => parentTab.name;

        public override void Click()
        {
            parentMenu.SwitchToTab(parentTab);
        }

        private void OnValidate()
        {
            parentMenu = GetComponentInParent<EnvironmentMenuWithTabs>(true);
        }
    }

#if UNITY_EDITOR
    [CanEditMultipleObjects]
    [CustomEditor(typeof(EnvironmentTabButton), editorForChildClasses: true)]
    public class EnvironmentTabButtonEditor : CellexalButtonEditor
    {
        public override int DrawMeshOrSpriteFields()
        {
            var buttonScript = target as EnvironmentTabButton;
            int popupChoice = base.DrawMeshOrSpriteFields();
            if (popupChoice == 0)
            {
                buttonScript.meshActivatedColor = EditorGUILayout.ColorField("Activated Color", buttonScript.meshActivatedColor);
                buttonScript.meshActivatedHighlightedColor = EditorGUILayout.ColorField("Activated And Highlighted Color", buttonScript.meshActivatedHighlightedColor);
            }
            else
            {
                buttonScript.meshActivatedTexture = (UnityEngine.Sprite)EditorGUILayout.ObjectField("Activated texture", buttonScript.meshActivatedTexture, typeof(UnityEngine.Sprite), true);
                buttonScript.meshActivatedHighlightedTexture = (UnityEngine.Sprite)EditorGUILayout.ObjectField("Activated And Highlighted texture", buttonScript.meshActivatedHighlightedTexture, typeof(UnityEngine.Sprite), true);
            }
            return popupChoice;
        }
    }
#endif
}
