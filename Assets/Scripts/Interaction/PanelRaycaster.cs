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
        public Material lockedNormalMaterial;
        public Material lockedHighlightMaterial;
        public Material correlatedGenesNormalMaterial;
        public Material correlatedGenesHighlightMaterial;
        public Material correlatedGenesPressedMaterial;


        private SteamVR_TrackedObject rightController;
        private ClickablePanel lastHit = null;
        private bool grabbingObject = false;

        private ControllerModelSwitcher controllerModelSwitcher;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            if (referenceManager == null)
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
            if (CrossSceneInformation.Normal)
            {
                rightController = referenceManager.rightController;
                controllerModelSwitcher = referenceManager.controllerModelSwitcher;
            }
            if (referenceManager.geneKeyboard)
            {
                referenceManager.geneKeyboard.SetMaterials(keyNormalMaterial, keyHighlightMaterial, keyPressedMaterial);

                Material newUnlockedNormalMaterial = new Material(unlockedNormalMaterial);
                Material newUnlockedHighlightMaterial = new Material(unlockedHighlightMaterial);
                Material newLockedNormalMaterial = new Material(lockedNormalMaterial);
                Material newLockedHighlightMaterial = new Material(lockedHighlightMaterial);

                foreach (var panel in GetComponentsInChildren<PreviousSearchesLock>(true))
                {
                    panel.SetMaterials(newUnlockedNormalMaterial, newUnlockedHighlightMaterial, newUnlockedHighlightMaterial, newLockedNormalMaterial, newLockedHighlightMaterial, newLockedHighlightMaterial, referenceManager.geneKeyboard.ScaleCorrection());
                }

                Material newCorrelatedGenesNormalMaterial = new Material(correlatedGenesNormalMaterial);
                Material newCorrelatedGenesHighlightMaterial = new Material(correlatedGenesHighlightMaterial);
                Material newCorrelatedGenesPressedMaterial = new Material(correlatedGenesPressedMaterial);

                foreach (var panel in GetComponentsInChildren<CorrelatedGenesPanel>(true))
                {
                    panel.SetMaterials(newCorrelatedGenesNormalMaterial, newCorrelatedGenesHighlightMaterial, newCorrelatedGenesPressedMaterial, referenceManager.geneKeyboard.ScaleCorrection());
                }
            }
            if (referenceManager.folderKeyboard)
            {
                referenceManager.folderKeyboard.SetMaterials(keyNormalMaterial, keyHighlightMaterial, keyPressedMaterial);
            }
            if (referenceManager.webBrowserKeyboard)
            {
                referenceManager.webBrowserKeyboard.SetMaterials(keyNormalMaterial, keyHighlightMaterial, keyPressedMaterial);
            }
            if (referenceManager.filterNameKeyboard)
            {
                referenceManager.filterNameKeyboard.SetMaterials(keyNormalMaterial, keyHighlightMaterial, keyPressedMaterial);
            }
            if (referenceManager.filterOperatorKeyboard)
            {
                referenceManager.filterOperatorKeyboard.SetMaterials(keyNormalMaterial, keyHighlightMaterial, keyPressedMaterial);
            }
            if (referenceManager.filterValueKeyboard)
            {
                referenceManager.filterValueKeyboard.SetMaterials(keyNormalMaterial, keyHighlightMaterial, keyPressedMaterial);
            }

            /*
            // tell all the panels which materials they should use
            foreach (var panel in GetComponentsInChildren<ClickableTextPanel>(true))
            {
                panel.SetMaterials(keyNormalMaterial, keyHighlightMaterial, keyPressedMaterial);
            }

            //foreach (var panel in GetComponentsInChildren<KeyboardPanel>(true))
            //{
            //    panel.SetMaterials(keyNormalMaterial, keyHighlightMaterial, keyPressedMaterial);
            //}

            foreach (var panel in GetComponentsInChildren<ClickableReportPanel>(true))
            {
                panel.SetMaterials(keyNormalMaterial, keyHighlightMaterial, keyPressedMaterial);
            }



            foreach (var panel in GetComponentsInChildren<ColoringOptionsPanel>(true))
            {
                panel.SetMaterials(keyNormalMaterial, keyHighlightMaterial, keyPressedMaterial);
            }

            foreach (var panel in GetComponentsInChildren<AnnotatePanel>(true))
            {
                panel.SetMaterials(keyNormalMaterial, keyHighlightMaterial, keyPressedMaterial);
            }

            foreach (var panel in GetComponentsInChildren<ExportAnnotationPanel>(true))
            {
                panel.SetMaterials(keyNormalMaterial, keyHighlightMaterial, keyPressedMaterial);
            }

            foreach (var panel in GetComponentsInChildren<ClearExpressionColoursPanel>(true))
            {
                panel.SetMaterials(keyNormalMaterial, keyHighlightMaterial, keyPressedMaterial);
            }
            */

            CellexalEvents.ObjectGrabbed.AddListener(() => grabbingObject = true);
            CellexalEvents.ObjectUngrabbed.AddListener(() => grabbingObject = false);
        }

        private void Update()
        {
            if (!CrossSceneInformation.Tutorial && !(CrossSceneInformation.Normal && controllerModelSwitcher.Ready() &&
                !grabbingObject && !referenceManager.selectionToolCollider.IsSelectionToolEnabled()))
                return;

            var raycastingSource = referenceManager.rightLaser.transform;
            var device = SteamVR_Controller.Input((int)rightController.index);
            var ray = new Ray(raycastingSource.position, raycastingSource.forward);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // if we hit something this frame.
                var hitPanel = hit.collider.transform.gameObject.GetComponent<ClickablePanel>();


                if (hitPanel != null)
                {
                    if (controllerModelSwitcher.ActualModel != ControllerModelSwitcher.Model.Keyboard)
                    {
                        controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Keyboard);
                    }
                    //referenceManager.laserPointerController.ToggleLaser(true);
                    referenceManager.laserPointerController.Override = true;
                    if (lastHit != null && lastHit != hitPanel)
                    {
                        lastHit.SetHighlighted(false);
                    }
                    hitPanel.SetHighlighted(true);
                    var keyboardHandler = hitPanel.GetComponentInParent<KeyboardHandler>();
                    Vector2 uv2 = keyboardHandler.ToUv2Coord(hit.point);

                    hitPanel.UpdateLaserCoords(uv2);

                    if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                    {
                        hitPanel.Click();
                        hitPanel.Pulse(uv2);
                    }

                    lastHit = hitPanel;
                }
                else if (lastHit != null)
                {
                    // if we hit something this frame but it was not a clickablepanel and we hit a clickablepanel last frame.
                    lastHit.SetHighlighted(false);
                    lastHit.UpdateLaserCoords(new Vector2(-1f, -1f));
                    lastHit = null;
                    controllerModelSwitcher.SwitchToDesiredModel();
                    //referenceManager.laserPointerController.ToggleLaser(false);

                    referenceManager.laserPointerController.Override = false;
                }

            }
            else if (lastHit != null)
            {
                controllerModelSwitcher.SwitchToDesiredModel();
                //referenceManager.laserPointerController.ToggleLaser(false);
                referenceManager.laserPointerController.Override = false;
                // if we hit nothing this frame, but hit something last frame.
                lastHit.SetHighlighted(false);
                lastHit.UpdateLaserCoords(new Vector2(-1f, -1f));
                lastHit = null;
            }

        }
    }
}
