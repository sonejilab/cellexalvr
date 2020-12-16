using UnityEngine;
using Valve.VR.InteractionSystem;

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
                { return "Teleport Back To Start"; }
            }
        }

        public override void Click()
        {
            playArea.position = Vector3.zero;
        }

        private void Start()
        {
            playArea = Player.instance.transform;
        }

    }
}
