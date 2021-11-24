using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace CellexalVR.Menu.Buttons.General
{
    /// <summary>
    /// Resets player and play area position.
    /// </summary>
    public class TeleportHomeButton : CellexalButton
    {

        protected override string Description
        {
            get
            {
                { return "Teleport Back To Start"; }
            }
        }

        public override void Click()
        {
            referenceManager.VRRig.transform.position = Vector3.zero;

        }

    }
}
