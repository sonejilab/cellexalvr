using UnityEngine;
using CellexalVR.General;
namespace CellexalVR.Interaction
{
    /// <summary>
    /// Responds to raycasting from a <see cref="CellexalRaycast"/> onto a group of <see cref="PresetClickableTextPanel"/>.
    /// Used for creating demo scenes.
    /// </summary>
    public class PresetClickablePanelRaycaster : CellexalRaycastable
    {
        public ReferenceManager referenceManager;

        private ClickablePanel lastHit = null;
        private bool hitDemoPanelLastFrame = false;
        private ControllerModelSwitcher controllerModelSwitcher;
        private int panelLayerMask;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
                canBePushedAndPulled = false;
            }
        }

        private void Start()
        {
            controllerModelSwitcher = referenceManager.controllerModelSwitcher;
            panelLayerMask = 1 << LayerMask.NameToLayer("SelectableLayer");
        }

        public override void OnRaycastHit(RaycastHit hitInfo, CellexalRaycast raycaster)
        {
            // if we hit something this frame.
            if (!hitDemoPanelLastFrame)
            {
                hitDemoPanelLastFrame = true;
                SetLaserActivated(true);
            }
            var hitPanel = hitInfo.collider.transform.gameObject.GetComponent<PresetClickableTextPanel>();
            if (hitPanel != null)
            {
                if (lastHit != null && lastHit != hitPanel)
                {
                    lastHit.SetHighlighted(false);
                }
                hitPanel.SetHighlighted(true);
                //if (device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
                //if (triggerClick)
                //{
                //    hitPanel.Click();
                //}
                lastHit = hitPanel;
            }
            else if (lastHit != null)
            {
                // if we hit something this frame but it was not a clickablepanel and we hit a clickablepanel last frame.
                lastHit.SetHighlighted(false);
                lastHit = null;
            }
            else if (lastHit != null)
            {
                // if we hit nothing this frame, but hit something last frame.
                lastHit.SetHighlighted(false);
                lastHit = null;
            }
            else if (hitDemoPanelLastFrame)
            {
                // if we hit nothing this frame and hit the panels last frame
                hitDemoPanelLastFrame = false;
                SetLaserActivated(false);
            }

        }

        private void SetLaserActivated(bool active)
        {
            if (controllerModelSwitcher.ActualModel != ControllerModelSwitcher.Model.Keyboard ||
                controllerModelSwitcher.ActualModel != ControllerModelSwitcher.Model.TwoLasers ||
                controllerModelSwitcher.ActualModel != ControllerModelSwitcher.Model.Menu)
            {
                //referenceManager.rightLaser.enabled = active;
                referenceManager.laserPointerController.Override = active;
                if (active)
                {
                    controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Menu);
                }
                else
                {
                    controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Normal);
                    //controllerModelSwitcher.ActivateDesiredTool();
                }
            }
        }
    }
}