using AnalysisLogic;
using CellexalVR.AnalysisLogic;
using CellexalVR.General;
using DG.Tweening;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

namespace CellexalVR.Spatial
{
    /// <summary>
    /// Class used to change the point cloud settings such as transparency, size and spread.
    /// The way to change the values is through a slider for each.
    /// Changing the settings changes the value for all point clouds in the scene.
    /// </summary>
    public class PointCloudSettings : MonoBehaviour
    {
        [SerializeField] private GameObject sizeSlider;
        [SerializeField] private GameObject spreadSlider;
        [SerializeField] private GameObject alphaSlider;
        [SerializeField] private UnityEvent<float> onSizeValueChanged;
        [SerializeField] private UnityEvent<float> onSpreadValueChanged;
        [SerializeField] private UnityEvent<float> onAlphaValueChanged;

        [SerializeField] private InputActionAsset actionAsset;
        [SerializeField] private InputActionReference actionReference;


        private bool toggled;
        private List<VisualEffect> pointClouds = new List<VisualEffect>();
        public float size = 0.003f;
        public float Size
        {
            get => size;
            set
            {
                size = value;
                onSizeValueChanged.Invoke(size);
            }
        }

        public float spread = 1f;
        public float Spread
        {
            get => spread;
            set
            {
                spread = value;
                onSpreadValueChanged.Invoke(spread);
            }
        }

        public float alpha = 1f;
        public float Alpha
        {
            get => alpha;
            set
            {
                alpha = value;
                onAlphaValueChanged.Invoke(alpha);
            }
        }

        private void Start()
        {
            CellexalEvents.GraphsLoaded.AddListener(PopulateList);
        }

        private void PopulateList()
        {
            foreach (PointCloud pc in PointCloudGenerator.instance.pointClouds)
            {
                pointClouds.Add(pc.GetComponent<VisualEffect>());
            }
        }

        private void OnActionClick(InputAction.CallbackContext context)
        {
            ToggleSettings();
        }

        /// <summary>
        /// Make the settings sliders visible and active to change.
        /// </summary>
        public void ToggleSettings()
        {
            if (!toggled)
            {
                Transform cameraTransform = ReferenceManager.instance.headset.transform;
                transform.position = cameraTransform.position + cameraTransform.forward * 0.7f;
                transform.LookAt(2 * transform.position - cameraTransform.position);
            }
            Vector3 targetScale = toggled ? Vector3.zero : Vector3.one;
            sizeSlider.transform.DOScale(targetScale, 0.8f).SetEase(Ease.OutBounce);
            spreadSlider.transform.DOScale(targetScale, 0.8f).SetEase(Ease.OutBounce);
            alphaSlider.transform.DOScale(targetScale, 0.8f).SetEase(Ease.OutBounce);
            toggled = !toggled;
        }

        /// <summary>
        /// Sets the point size of all point clouds in the scene.
        /// </summary>
        /// <param name="value">The size to set.</param>
        public void ChangePointSize(float value)
        {
            foreach (VisualEffect vfx in pointClouds)
            {
                vfx.SetFloat("Size", value);
                vfx.enabled = false;
                vfx.enabled = true;
            }
        }

        /// <summary>
        /// Sets the point spread of all point clouds in the scene.
        /// Spread means the distances between each point without changing size.
        /// It scales the same for all points so the structure of the graph remains.
        /// </summary>
        /// <param name="value"></param>
        public void ChangePointSpread(float value)
        {
            foreach (VisualEffect vfx in pointClouds)
            {
                vfx.SetFloat("Spread", value);
                vfx.enabled = false;
                vfx.enabled = true;
            }
        }

        /// <summary>
        /// Change transparency of all point clouds in the scene.
        /// </summary>
        /// <param name="value"></param>
        public void ChangePointAlpha(float value)
        {
            foreach (VisualEffect vfx in pointClouds)
            {
                vfx.SetFloat("Transparency", value);
                vfx.enabled = false;
                vfx.enabled = true;
            }
        }
    }
}