using CellexalVR.DesktopUI;
using UnityEngine;

namespace CellexalVR.SceneObjects
{

    /// <summary>
    /// This class turns of those pesky lights that are useful while in the editor.
    /// </summary>
    public class TurnOffThoseLights : MonoBehaviour
    {

        private void Start()
        {
            gameObject.SetActive(false);
        }

        [ConsoleCommand("turnOffThoseLights", "lights")]
        public void ToggleLights(bool on)
        {
            gameObject.SetActive(on);
        }
    }
}