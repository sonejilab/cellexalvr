﻿using CellexalVR.Menu.SubMenus;
using CellexalVR.General;
using UnityEngine;

namespace CellexalVR.Menu.Buttons
{
    /// <summary>
    /// Represents a button that opens a pop up menu.
    /// </summary>
    public class SubMenuButton : CellexalButton
    {
        public string description;
        public GameObject buttonsToDeactivate;
        public GameObject menu;
        public TMPro.TextMeshPro textMeshToDarken;

        private Tab activeTab;


        protected override string Description
        {
            get { return description; }
        }

        private void Start()
        {
            // The gameobject should be active but the renderers and colliders should be disabled.
            // This makes the buttons in the menu able to receive events while not being shown.
            menu.SetActive(true);
            //SetMenuActivated(false);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
            CellexalEvents.GraphsLoaded.AddListener(TurnOn);
        }

        public override void Click()
        {
            OpenMenu();
        }

        /// <summary>
        /// Opens the sub menu.
        /// </summary>
        public void OpenMenu()
        {
            textMeshToDarken.GetComponent<MeshRenderer>().enabled = false;
            var tabs = menu.GetComponentsInChildren<Tab>();
            if (tabs.Length > 0)
            {
                var firstTab = tabs[0];
                firstTab.SetTabActive(true);
                firstTab.tabButton.SetHighlighted(true);
                // menu.GetComponent<MenuWithTabs>().active = true;
            }
            var subMenu = menu.GetComponent<SubMenu>();
            if (subMenu != null)
            {
                subMenu.SetMenuActive(true);
            }
            descriptionText.text = "";
        }

        private void DeactivateButtonsRecursive(GameObject buttonsToDeactivate)
        {
            foreach (Transform t in buttonsToDeactivate.transform)
            {
                if (infoMenu != null)
                {
                    infoMenu.SetActive(false);
                }

                // if this is a button, deactivate it
                CellexalButton b = t.GetComponent<CellexalButton>();
                if (b != null)
                {
                    b.StoreState();
                    b.SetButtonActivated(false);
                }

                // recursive call to include all children of children
                DeactivateButtonsRecursive(t.gameObject);
            }
        }

        /// <summary>
        /// Show or hides the submenu
        /// </summary>
        /// <param name="activate"> True for showing the submenu, false for hiding. </param>
        public void SetMenuActivated(bool activate)
        {
            // Turn on or off the menu it self
            MenuWithTabs menuWithTabs = menu.GetComponent<MenuWithTabs>();
            if (menuWithTabs) menuWithTabs.Active = activate;
            Renderer menuRenderer = menu.GetComponent<Renderer>();
            if (menuRenderer)
                menuRenderer.enabled = activate;
            Collider menuCollider = menu.GetComponent<Collider>();
            if (menuCollider)
                menuCollider.enabled = activate;

            menu.GetComponent<SubMenu>().SetMenuActive(activate);
        }

        private void SetGameObjectAndChildrenEnabled(GameObject obj, bool active)
        {
            foreach (var r in obj.GetComponentsInChildren<Renderer>())
            {
                if (r.gameObject.name.Equals("ActiveOutline"))
                {
                    r.gameObject.SetActive(r.transform.parent.GetComponent<CellexalButton>().storedState);
                }

                r.enabled = active;
            }

            foreach (var c in obj.GetComponentsInChildren<Collider>())
            {
                c.enabled = active;
                if (c.gameObject.name.Equals("ActiveOutline"))
                {
                    c.gameObject.SetActive(c.transform.parent.GetComponent<CellexalButton>().storedState);
                }

                c.enabled = active;
            }
        }

        void TurnOn()
        {
            SetButtonActivated(true);
        }

        void TurnOff()
        {
            SetButtonActivated(false);
        }
    }
}