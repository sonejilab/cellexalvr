using UnityEngine;
using VRTK;

namespace CellexalVR.Menu.Buttons.General
{
    /// <summary>
    /// Resets player and play area position.
    /// </summary>
    public class TeleportHomeButton : CellexalButton
    {
        protected Transform playArea;

        protected override string Description
        {
            get
            {
                { return "Teleport back home"; }
            }
        }

        public override void Click()
        {
            playArea.position = Vector3.zero;
        }

        // Use this for initialization
        void Start()
        {
            playArea = VRTK_DeviceFinder.PlayAreaTransform();
        }

    }
}
