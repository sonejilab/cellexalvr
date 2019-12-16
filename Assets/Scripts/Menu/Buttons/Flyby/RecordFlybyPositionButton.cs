using CellexalVR.Menu.SubMenus;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.Flyby
{
    public class RecordFlybyPositionButton : CellexalButton
    {

        public FlybyMenu flybyMenu;

        private Transform headset;

        protected override string Description => "Record new flyby position";

        protected override void Awake()
        {
            base.Awake();
            headset = referenceManager.headset.transform;
            flybyMenu = gameObject.GetComponentInParent<FlybyMenu>();
        }

        public override void Click()
        {
            flybyMenu.RecordPosition(headset.position, headset.rotation);
        }
    }
}