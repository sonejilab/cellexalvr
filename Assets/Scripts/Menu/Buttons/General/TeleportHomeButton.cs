using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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

        // Use this for initialization
        void Start()
        {
            // OpenXR find play area reference.
            //playArea = VRTK_DeviceFinder.PlayAreaTransform();
        }

    }
}
