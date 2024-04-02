using CellexalVR.Interaction;
using UnityEngine;

namespace CellexalVR.AnalysisObjects
{
    public class EnvironmentTab : CellexalRaycastable
    {
        public EnvironmentTabButton tabButton;
        public GameObject contentParent;
        public EnvironmentMenuWithTabs parentMenu;

        private void OnValidate()
        {
            parentMenu = GetComponentInParent<EnvironmentMenuWithTabs>();
        }

        /// <summary>
        /// Enables or disables the content gameobject of this tab.
        /// </summary>
        /// <param name="active">True if the tab should be enabled, false if it should be disabled.</param>
        public void SetTabActive(bool active)
        {
            contentParent.SetActive(active);
            tabButton.SetTabActivated(active);
        }
    }
}
