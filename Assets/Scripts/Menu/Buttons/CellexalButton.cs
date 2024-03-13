using CellexalVR.General;
using CellexalVR.Interaction;
using CellexalVR.Menu.Buttons.General;
using CellexalVR.Menu.SubMenus;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Menu.Buttons
{
    /// <summary>
    /// Abstract general purpose class that represents a button on the menu.
    /// </summary>
    public abstract class CellexalButton : CellexalRaycastable
    {
        public ReferenceManager referenceManager;
        public TMPro.TextMeshPro descriptionText;
        public GameObject infoMenu;
        public GameObject activeOutline;
        // all buttons must override this variable's get property
        /// <summary>
        /// A string that briefly explains what this button does.
        /// </summary>
        abstract protected string Description
        {
            get;
        }

        // These are drawn in the inspector through CellexalButtonEditor
        [HideInInspector]
        public Color meshStandardColor = Color.black;
        [HideInInspector]
        public Color meshHighlightColor = Color.white;
        [HideInInspector]
        public Color meshDeactivatedColor = Color.grey;
        [HideInInspector]
        public Sprite standardTexture = null;
        [HideInInspector]
        public Sprite highlightedTexture = null;
        [HideInInspector]
        public Sprite deactivatedTexture = null;
        [HideInInspector]
        public int popupChoice = 0;

        protected SpriteRenderer spriteRenderer;
        protected MeshRenderer meshRenderer;
        [HideInInspector]
        public bool storedState;
        public bool controllerInside = false;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
                if (OnActivate is null)
                {
                    OnActivate = new UnityEngine.Events.UnityEvent();
                }
                if (!CellexalEvents.IsPersistentListenerAlreadyAdded(OnActivate, Click))
                {
                    UnityEditor.Events.UnityEventTools.AddPersistentListener(OnActivate, Click);
                }

                canBePushedAndPulled = false;
            }
        }
#endif

        protected virtual void Awake()
        {
            if (referenceManager == null)
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
            spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
            meshRenderer = gameObject.GetComponent<MeshRenderer>();

        }

        public override void OnRaycastEnter()
        {
            base.OnRaycastEnter();
            controllerInside = true;
            SetHighlighted(true);
        }

        public override void OnRaycastExit()
        {
            base.OnRaycastExit();
            controllerInside = false;
            SetHighlighted(false);
            if (descriptionText.text == Description)
            {
                descriptionText.text = "";
            }
        }

        /// <summary>
        /// Handles what happens when the user points the controller towards the button and presses the trigger.
        /// </summary>
        public abstract void Click();

        protected virtual void HelpClick()
        {
            if (!infoMenu) return;

            infoMenu.GetComponent<VideoButton>().StartVideo();
        }

        public virtual void SetButtonActivated(bool activate)
        {
            if (!activate)
            {
                descriptionText.text = "";
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = deactivatedTexture;
                }
                else if (meshRenderer != null)
                {
                    meshRenderer.material.color = meshDeactivatedColor;
                }
            }

            if (activate)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = standardTexture;
                }
                else if (meshRenderer != null)
                {
                    meshRenderer.material.color = meshStandardColor;
                }
            }

            active = activate;
        }

        public void StoreState()
        {
            storedState = active;
        }

        /// <summary>
        /// To synchronise the outline in multi-user mode. So the outline doesnt get active unless the other users menu or tab is active.
        /// </summary>
        public void ToggleOutline(bool toggle, bool legend = false)
        {
            var tab = transform.parent.GetComponent<Tab>();
            var menuNoTab = transform.parent.GetComponent<SubMenu>();
            if (tab != null && tab.Active)
            {
                activeOutline.SetActive(toggle);
            }
            else if (menuNoTab != null || legend)
            {
                activeOutline.SetActive(toggle);
            }

            storedState = toggle;
        }

        // In case OnTriggerExit doesnt get called by laser pointer we need to manually do the unhighlighting.
        protected void Exit()
        {
            //print(name + " exit");
            if (descriptionText.text == Description)
            {
                descriptionText.text = "";
            }

            controllerInside = false;
            if (active)
            {
                SetHighlighted(false);
            }
            else
            {
                SetButtonActivated(false);
            }
            if (infoMenu)
            {
                infoMenu.SetActive(false);
            }
        }

        public virtual void SetHighlighted(bool highlight)
        {
            if (highlight)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = highlightedTexture;
                }
                else if (meshRenderer != null)
                {
                    meshRenderer.material.color = meshHighlightColor;
                }

                if (descriptionText.text == "")
                {
                    descriptionText.text = Description;
                }
            }

            if (!highlight)
            {
                if (spriteRenderer != null)
                {
                    spriteRenderer.sprite = standardTexture;
                }
                else if (meshRenderer != null)
                {
                    meshRenderer.material.color = meshStandardColor;
                }
            }

            if (infoMenu)
            {
                infoMenu.SetActive(highlight);
            }

            controllerInside = highlight;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// A custom inspector for all buttons to help show only fields that are used.
    /// </summary>
    [CustomEditor(typeof(CellexalButton), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class CellexalButtonEditor : Editor
    {
        public string[] buttonTypeOptions = new string[] { "Mesh", "Sprite" };

        public virtual int DrawMeshOrSpriteFields()
        {
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
            return buttonScript.popupChoice;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            DrawMeshOrSpriteFields();
            base.OnInspectorGUI();

            serializedObject.ApplyModifiedProperties();
        }

    }
#endif
}
