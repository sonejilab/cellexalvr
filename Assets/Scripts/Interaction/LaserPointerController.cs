using CellexalVR.General;
using CellexalVR.Menu.Buttons;
using UnityEngine;
using Valve.VR;
using VRTK;

namespace CellexalVR.Interaction
{
    /// <summary>
    /// Handles the laser pointer on right controller. Laser activates when controller is pointed towards the menu or 
    /// something on the menulayer. Deactivated otherwise unless laser tool is active.
    /// </summary>
    public class LaserPointerController : MonoBehaviour
    {
        private int frame;
        private int layerMaskEnv;
        private int layerMaskMenu;
        private int environmentButtonLayer;
        private int menuLayer;
        private int keyboardLayer;

        private int layerMaskController;

        //private int layerMaskGraph;
        //private int layerMaskNetwork;
        //private int layerMaskOther;
        private bool keyboardActive;
        private bool touchingObject;
        private bool hitLastFrame;
        private ControllerModelSwitcher controllerModelSwitcher;

        public ReferenceManager referenceManager;
        public Transform origin;
        public bool Override { get; set; }
        public bool alwaysActive;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        // Use this for initialization
        private void Start()
        {
            frame = 0;
            referenceManager.rightControllerScriptAlias.GetComponent<VRTK_StraightPointerRenderer>().enabled = false;
            environmentButtonLayer = LayerMask.NameToLayer("EnvironmentButtonLayer");
            keyboardLayer = LayerMask.NameToLayer("KeyboardLayer");
            menuLayer = LayerMask.NameToLayer("MenuLayer");
            layerMaskMenu = 1 << menuLayer;
            layerMaskEnv = 1 << environmentButtonLayer | 1 << keyboardLayer;
            controllerModelSwitcher = referenceManager.controllerModelSwitcher;
            //CellexalEvents.ObjectGrabbed.AddListener(() => TouchingObject(true));
            //CellexalEvents.ObjectUngrabbed.AddListener(() => TouchingObject(false));
        }

        private void Update()
        {
            frame++;
            if (frame < 5) return;
            frame = 0;
            Frame5Update();
        }

        // Call every 5th frame.
        private void Frame5Update()
        {
            RaycastHit hit;
            origin.localRotation = Quaternion.Euler(25f, 0, 0);
            Physics.Raycast(origin.position, origin.forward, out hit, 10, layerMaskMenu);
            if (hit.collider && hit.collider.tag != "BlockLaser")
            {
                // if we hit a button in the menu
                referenceManager.rightLaser.enabled = true;
                if (controllerModelSwitcher.ActualModel != ControllerModelSwitcher.Model.Menu)
                {
                    controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Menu);
                }
                return;
            }
            origin.localRotation = Quaternion.Euler(0f, 0, 0);
            Physics.Raycast(origin.position, origin.forward, out hit, 10, layerMaskEnv);
            if (hit.collider && hit.collider.tag != "BlockLaser" &&
                controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.Normal ||
                controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.Keyboard)
            
            {
                // if we hit a button in the environment (keyboard or env button)
                // if (controllerModelSwitcher.ActualModel != ControllerModelSwitcher.Model.Keyboard)
                // {
                //     controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Keyboard);
                // }
                if (controllerModelSwitcher.DesiredModel != controllerModelSwitcher.ActualModel)
                {
                    controllerModelSwitcher.ActivateDesiredTool();
                }
                referenceManager.multiuserMessageSender.SendMessageToggleLaser(true);
                referenceManager.rightLaser.enabled = true;
                referenceManager.rightLaser.tracerVisibility = VRTK_BasePointerRenderer.VisibilityStates.AlwaysOn;
                referenceManager.multiuserMessageSender.SendMessageMoveLaser(origin, hit.point);
                return;
            }
            if (alwaysActive)
            {
                Vector3 hitPoint;
                if (!hit.collider)
                {
                    hitPoint = origin.position + (origin.forward * 10);
                }
                else
                {
                    hitPoint = hit.point;
                }
                referenceManager.multiuserMessageSender.SendMessageMoveLaser(origin, hitPoint);
                if (controllerModelSwitcher.DesiredModel != controllerModelSwitcher.ActualModel)
                {
                    controllerModelSwitcher.ActivateDesiredTool();
                }
            }
            else if (referenceManager.rightLaser.enabled &&
                     controllerModelSwitcher.ActualModel != ControllerModelSwitcher.Model.Keyboard) 
            {
                referenceManager.rightLaser.tracerVisibility = VRTK_BasePointerRenderer.VisibilityStates.AlwaysOff;
                referenceManager.rightLaser.enabled = false;
                referenceManager.multiuserMessageSender.SendMessageToggleLaser(false);
            }

            if (alwaysActive || Override) return;
            if (hit.collider) return;
            if (controllerModelSwitcher.DesiredModel != controllerModelSwitcher.ActualModel)
            {
                controllerModelSwitcher.ActivateDesiredTool();
            }
        }

        // Toggle Laser from laser button. Laser should then be active until toggled off.
        public void ToggleLaser(bool active)
        {
            alwaysActive = active;
            referenceManager.rightLaser.tracerVisibility = active
                ? VRTK_BasePointerRenderer.VisibilityStates.AlwaysOn
                : VRTK_BasePointerRenderer.VisibilityStates.AlwaysOff;

            //referenceManager.rightLaser.enabled = active;
            if (controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.TwoLasers)
                referenceManager.leftLaser.tracerVisibility = VRTK_BasePointerRenderer.VisibilityStates.AlwaysOn;
            origin.localRotation = Quaternion.identity;
            referenceManager.multiuserMessageSender.SendMessageToggleLaser(active);
        }

        private void TouchingObject(bool touch)
        {
            referenceManager.rightLaser.enabled = !touch;
            if (controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.TwoLasers)
            {
                referenceManager.leftLaser.enabled = !touch;
            }

            touchingObject = touch;
        }
    }
}