using CellexalVR.Menu.SubMenus;

namespace CellexalVR.Menu.Buttons.Flyby
{
    public class RenderFlybyButton : CellexalButton
    {
        public FlybyMenu flybyMenu;

        protected override string Description => "Render flyby";

        public override void Click()
        {
            flybyMenu.RenderFlyby();
        }
    }
}