using CellexalVR.Menu.SubMenus;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.Flyby
{
    public class ResetFlybyPositionsButton : CellexalButton
    {
        public FlybyMenu flybyMenu;

        protected override string Description => "Reset flyby positions";

        private void Start()
        {
            flybyMenu = gameObject.GetComponentInParent<FlybyMenu>();
        }

        public override void Click()
        {
            flybyMenu.ResetPositions();
        }
    }
}
