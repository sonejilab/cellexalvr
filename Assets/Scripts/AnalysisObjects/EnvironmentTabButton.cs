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
        public Color meshTabActivatedColor;
        [HideInInspector]
        public Color meshTabActivatedHighlightedColor;
        [HideInInspector]
        public Sprite tabActivatedTexture;
        [HideInInspector]
        public Sprite tabActivatedHighlightedTexture;

        private bool tabActivated;

        protected override string Description => parentTab.name;

        public override void Click()
        {
            parentMenu.SwitchToTab(parentTab);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            parentMenu = GetComponentInParent<EnvironmentMenuWithTabs>(true);
        }
#endif

        public void SetTabActivated(bool activated)
        {
            tabActivated = activated;
            SetButtonActivated(active);
        }

        public override void SetButtonActivated(bool activate)
        {
            if (!tabActivated || !activate)
            {
                base.SetButtonActivated(activate);
            }
            else
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = tabActivatedTexture;
                }
                else if (meshRenderer != null)
                {
                    meshRenderer.material.color = meshTabActivatedColor;
                }
            }
        }

        public override void SetHighlighted(bool highlight)
        {
            if (!tabActivated)
            {
                base.SetHighlighted(highlight);
            }
            else if (highlight)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = tabActivatedHighlightedTexture;
                }
                else if (meshRenderer != null)
                {
                    meshRenderer.material.color = meshTabActivatedHighlightedColor;
                }
            }
            else
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = tabActivatedTexture;
                }
                else if (meshRenderer != null)
                {
                    meshRenderer.material.color = meshTabActivatedColor;
                }
            }
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
                buttonScript.meshTabActivatedColor = EditorGUILayout.ColorField("Activated Color", buttonScript.meshTabActivatedColor);
                buttonScript.meshTabActivatedHighlightedColor = EditorGUILayout.ColorField("Activated And Highlighted Color", buttonScript.meshTabActivatedHighlightedColor);
            }
            else
            {
                buttonScript.tabActivatedTexture = (UnityEngine.Sprite)EditorGUILayout.ObjectField("Activated texture", buttonScript.tabActivatedTexture, typeof(UnityEngine.Sprite), true);
                buttonScript.tabActivatedHighlightedTexture = (UnityEngine.Sprite)EditorGUILayout.ObjectField("Activated And Highlighted texture", buttonScript.tabActivatedHighlightedTexture, typeof(UnityEngine.Sprite), true);
            }
            return popupChoice;
        }
    }
#endif
}
