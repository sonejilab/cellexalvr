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

        public void SetTabActive(bool active)
        {
            contentParent.SetActive(active);
            tabButton.SetHighlighted(active);
        }
    }
}
