using CellexalVR.General;
using CellexalVR.Menu.SubMenus;
using UnityEngine;

namespace CellexalVR.Menu.Buttons
{
    /// <summary>
    /// Represents the tab buttons on top of a tab.
    /// </summary>
    public class TabButton : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public Tab tab;
        public MenuWithTabs Menu;

        protected SteamVR_TrackedObject rightController;
        protected bool controllerInside = false;
        protected SteamVR_Controller.Device device;
        private MeshRenderer meshRenderer;
        private Color standardColor = Color.black;
        private Color highlightColor = Color.blue;
        public bool highlight;

        protected virtual void Start()
        {
            meshRenderer = GetComponent<MeshRenderer>();
            meshRenderer.material.color = standardColor;
            rightController = referenceManager.rightController;
            this.tag = "Menu Controller Collider";
        }

        protected virtual void Update()
        {
            device = SteamVR_Controller.Input((int)rightController.index);
            if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                Menu.TurnOffAllTabs();
                tab.SetTabActive(true);
                highlight = true;
                SetHighlighted(highlight);
            }
            if (!tab.Active && highlight && !controllerInside)
            {
                highlight = false;
                SetHighlighted(highlight);
            }
        }

        protected void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Menu Controller Collider") || other.gameObject.name == "[RightController]BasePointerRenderer_ObjectInteractor_Collider")
            {
                highlight = true;
                controllerInside = true;
                SetHighlighted(highlight);
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Menu Controller Collider") || other.gameObject.name == "[RightController]BasePointerRenderer_ObjectInteractor_Collider")
            {
                controllerInside = false;
                if (!tab.Active)
                {
                    highlight = false;
                    SetHighlighted(highlight);
                }
            }
        }

        /// <summary>
        /// Changes the color of the button to either its highlighted color or standard color.
        /// </summary>
        /// <param name="highlight"> True if the button should be highlighted, false otherwise. </param>
        public virtual void SetHighlighted(bool h)
        {
            if (h)
            {
                meshRenderer.material.color = highlightColor;
            }
            else
            {
                meshRenderer.material.color = standardColor;
            }
        }
    }
}