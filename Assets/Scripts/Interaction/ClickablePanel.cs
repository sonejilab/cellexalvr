using CellexalVR.General;
using System.Collections;
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
        private static Vector4 PulseAndLaserCoords;

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

        public abstract void Click();

        /// <summary>
        /// Displays a pulse on this and neighbouring panels.
        /// </summary>
        /// <param name="pos">The uv2 coordinates of the center of the pulse.</param>
        public void Pulse(Vector2 pos)
        {
            PulseAndLaserCoords = new Vector4(pos.x, pos.y, PulseAndLaserCoords.z, PulseAndLaserCoords.w);
            if (isActiveAndEnabled)
            {
                StartCoroutine(PulseCoroutine());
            }
        }

        /// <summary>
        /// Updates the coordinates for the laser hit animation.
        /// </summary>
        /// <param name="pos">The uv2 coordinates of the laser hit.</param>
        public void UpdateLaserCoords(Vector2 pos)
        {
            PulseAndLaserCoords = new Vector4(PulseAndLaserCoords.x, PulseAndLaserCoords.y, pos.x, pos.y);
            keyNormalMaterial.SetVector("_PulseCoords", PulseAndLaserCoords);
            keyHighlightMaterial.SetVector("_PulseCoords", PulseAndLaserCoords);
            keyPressedMaterial.SetVector("_PulseCoords", PulseAndLaserCoords);
        }

        /// <summary>
        /// Uses the Keyboard shader to play a pulse anmimation.
        /// </summary>
        protected IEnumerator PulseCoroutine()
        {

            float t = 0f;
            float pulseDuration = keyNormalMaterial.GetFloat("_PulseDuration");
            keyNormalMaterial.SetVector("_PulseCoords", PulseAndLaserCoords);
            keyHighlightMaterial.SetVector("_PulseCoords", PulseAndLaserCoords);
            keyPressedMaterial.SetVector("_PulseCoords", PulseAndLaserCoords);
            while (t < pulseDuration)
            {
                keyNormalMaterial.SetFloat("_PulseStartTime", t);
                keyHighlightMaterial.SetFloat("_PulseStartTime", t);
                keyPressedMaterial.SetFloat("_PulseStartTime", t);
                t += Time.deltaTime;
                yield return null;
            }

            keyNormalMaterial.SetFloat("_PulseStartTime", -1f);
            keyHighlightMaterial.SetFloat("_PulseStartTime", -1f);
            keyPressedMaterial.SetFloat("_PulseStartTime", -1f);
        }

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