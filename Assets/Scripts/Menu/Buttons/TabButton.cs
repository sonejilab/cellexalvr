using CellexalVR.General;
using CellexalVR.Interaction;
using CellexalVR.Menu.SubMenus;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Menu.Buttons
{
    /// <summary>
    /// Represents the tab buttons on top of a tab.
    /// </summary>
    public class TabButton : CellexalRaycastable
    {
        public ReferenceManager referenceManager;
        public Tab tab;
        public MenuWithTabs Menu;

        private InputDevice device;
        [SerializeField] protected bool controllerInside = false;
        private MeshRenderer meshRenderer;
        private Color standardColor = Color.grey;
        private Color highlightColor = Color.blue;
        private bool highlight;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        protected virtual void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material.color = standardColor;
            OnActivate.AddListener(OnClick);
        }

        public override void OnRaycastEnter()
        {
            base.OnRaycastEnter();
            SetHighlighted(true);
        }

        public override void OnRaycastExit()
        {
            base.OnRaycastExit();
            SetHighlighted(false);
        }

        private void OnClick()
        {
            Menu.TurnOffAllTabs();
            tab.SetTabActive(true);
            highlight = true;
            SetHighlighted(highlight);
            controllerInside = false;
        }

        /// <summary>
        /// Changes the color of the button to either its highlighted color or standard color.
        /// </summary>
        /// <param name="highlight"> True if the button should be highlighted, false otherwise. </param>
        public virtual void SetHighlighted(bool h)
        {
            highlight = h;
            if (meshRenderer == null)
            {
                meshRenderer = GetComponent<MeshRenderer>();
            }

            meshRenderer.material.color = h ? highlightColor : standardColor;
        }
    }
}
