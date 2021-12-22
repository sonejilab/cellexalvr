using CellexalVR.General;
using CellexalVR.Menu.SubMenus;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CellexalVR.Menu.Buttons
{
    /// <summary>
    /// Represents a button that closes a menu that is opened on top of the main menu.
    /// </summary>
    public class CloseMenuButton : CellexalButton
    {
        public SubMenu menuToClose;

        public bool deactivateMenu = false;

        protected override string Description
        {
            get { return "Close menu"; }
        }

        public override void Click()
        {
            spriteRenderer.sprite = standardTexture;
            controllerInside = false;
            descriptionText.text = "";
            CloseMenu();
        }

        public void CloseMenu()
        {
            menuToClose.SetMenuActive(false);
            CellexalEvents.MenuClosed.Invoke();
        }
    }
}