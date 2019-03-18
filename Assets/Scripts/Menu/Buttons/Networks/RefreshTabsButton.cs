using CellexalVR.General;
using CellexalVR.Menu.SubMenus;
using UnityEngine;
namespace CellexalVR.Menu.Buttons.Networks
{
    /// <summary>
    /// Button that calls the refresh tabs function which adds new tabs to the menu if a new network has been created.
    /// </summary>
    public class RefreshTabsButton : CellexalButton
    {

        public SubMenuButton arcsMenuButton;
        public ToggleArcsSubMenu toggleArcsMenu;

        protected override string Description
        {
            get { return "Refresh Tabs"; }
        }

        protected void Start()
        {
            SetButtonActivated(true);
        }

        public override void Click()
        {
            referenceManager.arcsSubMenu.RefreshTabs();
            foreach (Renderer r in toggleArcsMenu.GetComponentsInChildren<Renderer>())
                r.enabled = false;
            foreach (Collider c in toggleArcsMenu.GetComponentsInChildren<Collider>())
                c.enabled = false;

            CellexalEvents.MenuClosed.Invoke();

            arcsMenuButton.SetMenuActivated(true);
        }

        private void TurnOn()
        {
            SetButtonActivated(true);
        }

        private void TurnOff()
        {
            SetButtonActivated(false);
        }
    }
}