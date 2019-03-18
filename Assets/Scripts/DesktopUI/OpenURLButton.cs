using UnityEngine;

namespace CellexalVR.DesktopUI
{
    /// <summary>
    /// Represents a button in the settings menu that opens a url.
    /// </summary>
    public class OpenURLButton : MonoBehaviour
    {
        public string url;

        public void Click()
        {
            Application.OpenURL(url);
        }
    }
}
