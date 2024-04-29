using CellexalVR.Menu.Buttons;

namespace CellexalVR.Menu.SubMenus
{

    public class ChangePageButton : CellexalButton
    {
        public int dir;
        private AttributeSubMenu attributeSubMenu;
        protected override string Description
        {
            get { return " "; }
        }

        private void Start()
        {
            attributeSubMenu = GetComponentInParent<AttributeSubMenu>();
        }

        public override void Click()
        {
            attributeSubMenu.ChangePage(dir);
        }
    }
}
