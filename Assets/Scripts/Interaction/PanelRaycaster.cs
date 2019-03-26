using CellexalVR.General;
using UnityEngine;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Handles the rayvasting on the panels around the keyboard.
    /// </summary>
    public class PanelRaycaster : MonoBehaviour
    {
        public ReferenceManager referenceManager;

        // materials used by buttons
        public Material keyNormalMaterial;
        public Material keyHighlightMaterial;
        public Material keyPressedMaterial;
        public Material unlockedNormalMaterial;
        public Material unlockedHighlightMaterial;
        public Material unlockedPressedMaterial;
        public Material lockedNormalMaterial;
        public Material lockedHighlightMaterial;
        public Material lockedPressedMaterial;
        public Material correlatedGenesNormalMaterial;
        public Material correlatedGenesHighlightMaterial;
        public Material correlatedGenesPressedMaterial;


        private SteamVR_TrackedObject rightController;
        private ClickablePanel lastHit = null;

        private ControllerModelSwitcher controllerModelSwitcher;

        private void Start()
        {
            if (referenceManager == null)
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
            rightController = referenceManager.rightController;
            controllerModelSwitcher = referenceManager.controllerModelSwitcher;
            referenceManager.keyboardHandler.SetMaterials(keyNormalMaterial, keyHighlightMaterial, keyPressedMaterial);
            referenceManager.folderKeyboard.SetMaterials(keyNormalMaterial, keyHighlightMaterial, keyPressedMaterial);
            referenceManager.webBrowserKeyboard.SetMaterials(keyNormalMaterial, keyHighlightMaterial, keyPressedMaterial);
            // tell all the panels which materials they should use
            foreach (var panel in GetComponentsInChildren<ClickableTextPanel>(true))
            {
                panel.SetMaterials(keyNormalMaterial, keyHighlightMaterial, keyPressedMaterial);
            }

            foreach (var panel in GetComponentsInChildren<ClickableReportPanel>(true))
            {
                panel.SetMaterials(keyNormalMaterial, keyHighlightMaterial, keyPressedMaterial);
            }

            foreach (var panel in GetComponentsInChildren<CorrelatedGenesPanel>(true))
            {
                panel.SetMaterials(correlatedGenesNormalMaterial, correlatedGenesHighlightMaterial, correlatedGenesPressedMaterial);
            }

            foreach (var panel in GetComponentsInChildren<PreviousSearchesLock>(true))
            {
                panel.SetMaterials(unlockedNormalMaterial, unlockedHighlightMaterial, unlockedPressedMaterial, lockedNormalMaterial, lockedHighlightMaterial, lockedPressedMaterial);
            }

            foreach (var panel in GetComponentsInChildren<ColoringOptionsPanel>(true))
            {
                panel.SetMaterials(keyNormalMaterial, keyHighlightMaterial, keyPressedMaterial);
            }

        }

        private void OnValidate()
        {
            if (gameObject.activeInHierarchy)
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }

        private void Update()
        {
            var raycastingSource = referenceManager.rightLaser.transform;
            var device = SteamVR_Controller.Input((int)rightController.index);
            var ray = new Ray(raycastingSource.position, raycastingSource.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                // if we hit something this frame.
                var hitPanel = hit.collider.transform.gameObject.GetComponent<ClickablePanel>();


                if (hitPanel != null)
                {
                    controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Keyboard);
                    referenceManager.laserPointerController.Override = true;
                    if (lastHit != null && lastHit != hitPanel)
                    {
                        lastHit.SetHighlighted(false);
                    }
                    hitPanel.SetHighlighted(true);
                    if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                    {
                        hitPanel.Click();
                    }

                    lastHit = hitPanel;
                }
                else if (lastHit != null)
                {
                    // if we hit something this frame but it was not a clickablepanel and we hit a clickablepanel last frame.
                    lastHit.SetHighlighted(false);
                    lastHit = null;
                    controllerModelSwitcher.SwitchToDesiredModel();
                    referenceManager.laserPointerController.Override = false;
                }

            }
            else if (lastHit != null)
            {
                controllerModelSwitcher.SwitchToDesiredModel();
                referenceManager.laserPointerController.Override = false;
                // if we hit nothing this frame, but hit something last frame.
                lastHit.SetHighlighted(false);
                lastHit = null;
            }

        }
    }
}