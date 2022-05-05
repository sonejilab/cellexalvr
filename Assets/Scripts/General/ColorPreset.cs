using UnityEngine;

namespace Assets.Scripts.General
{
    /// <summary>
    /// Saves and attaches a color to a gameobject so it can be used by other scripts later.
    /// </summary>
    public class ColorPreset : MonoBehaviour
    {
        public Color color = new Color(0, 0, 0);
    }
}