using CellexalVR.General;
using UnityEngine;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Abstract class that all panels around the keyboard should inherit.
    /// </summary>
    public abstract class ClickablePanel : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        // vector used by the shader to display pulse when a panel is clicked and an animation where the laser is.
        protected static Vector4 PulseAndLaserCoords;

        public Vector2 CenterUV { get; set; }
        protected new Renderer renderer;
        protected Material keyNormalMaterial;
        protected Material keyHighlightMaterial;
        protected Material keyPressedMaterial;
        private bool isPressed;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        protected virtual void Start()
        {
            renderer = gameObject.GetComponent<Renderer>();
            renderer.sharedMaterial = keyNormalMaterial;
        }

        /// <summary>
        /// Sets this panel's materials.
        /// </summary>
        /// <param name="keyNormalMaterial">The normal material.</param>
        /// <param name="keyHighlightMaterial">The material that should be used when the laser pointer is pointed at the button.</param>
        /// <param name="keyPressedMaterial">The material that should be used when the panel is pressed.</param>
        public virtual void SetMaterials(Material keyNormalMaterial, Material keyHighlightMaterial, Material keyPressedMaterial, Vector4 scaleCorrection)
        {
            this.keyNormalMaterial = keyNormalMaterial;
            this.keyHighlightMaterial = keyHighlightMaterial;
            this.keyPressedMaterial = keyPressedMaterial;


            this.keyNormalMaterial.SetVector("_ScaleCorrection", scaleCorrection);
            this.keyHighlightMaterial.SetVector("_ScaleCorrection", scaleCorrection);
            this.keyPressedMaterial.SetVector("_ScaleCorrection", scaleCorrection);

            if (renderer)
            {
                renderer.sharedMaterial = keyNormalMaterial;
            }
        }

        /// <summary>
        /// Called when the user points the controller towards this panel and presses the trigger.
        /// </summary>
        public abstract void Click();


        /// <summary>
        /// Sets this panel to highlighted or not highlighted.
        /// </summary>
        /// <param name="highlight">True for highlighted, false for not highlighted.</param>
        public virtual void SetHighlighted(bool highlight)
        {
            if (!renderer)
            {
                renderer = gameObject.GetComponent<Renderer>();
            }
            if (highlight && !isPressed)
            {
                renderer.sharedMaterial = keyHighlightMaterial;
            }
            else if (!highlight && isPressed)
            {
                renderer.sharedMaterial = keyPressedMaterial;
            }
            else if (!highlight)
            {
                renderer.sharedMaterial = keyNormalMaterial;
            }
        }

        /// <summary>
        /// Sets this panel to pressed or not pressed.
        /// </summary>
        /// <param name="pressed">True for pressed, false for not pressed.</param>
        public virtual void SetPressed(bool pressed)
        {
            isPressed = pressed;
            if (!renderer)
            {
                renderer = gameObject.GetComponent<Renderer>();
            }
            if (pressed)
            {
                renderer.sharedMaterial = keyPressedMaterial;
            }
            else
            {
                renderer.sharedMaterial = keyNormalMaterial;
            }
        }
    }
}
