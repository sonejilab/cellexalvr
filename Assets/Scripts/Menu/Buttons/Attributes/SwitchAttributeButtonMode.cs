using CellexalVR.Menu.SubMenus;

namespace CellexalVR.Menu.Buttons.Attributes
{
    /// <summary>
    /// Changes the appearance and functionality of the attribute buttons. 
    /// It now colours all the points that are of any of the attributes selected to red.
    /// </summary>
    public class SwitchAttributeButtonMode : CellexalButton
    {
        protected override string Description
        {
            get { return "Switch button appearance"; }
        }

        protected AttributeSubMenu attributeMenu;

        private void Start()
        {
            attributeMenu = referenceManager.attributeSubMenu;
        }

        public override void Click()
        {
            attributeMenu.SwitchButtonStates();
        }
    }
}