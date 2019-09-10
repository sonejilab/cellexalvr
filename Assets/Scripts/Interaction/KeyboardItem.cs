using UnityEngine;

namespace CellexalVR.Interaction
{

    [DisallowMultipleComponent]
    public class KeyboardItem : MonoBehaviour
    {
        public Vector2 position;
        public Vector2 size = new Vector2(1f, 1f);
        public bool hasKeyboardMaterial = true;
    }
}