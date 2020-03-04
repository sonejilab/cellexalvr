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
        private GameObject tempHit;
        private int layerMaskMenu;
        private int layerMaskKeyboard;

        private int layerMaskController;

        //private int layerMaskGraph;
        //private int layerMaskNetwork;
        //private int layerMaskOther;
        private bool keyboardActive;
        private bool touchingObject;
        private bool hitLastFrame;
        private ControllerModelSwitcher controllerModelSwitcher;
        private int environmentButtonLayer;
        private int menuLayer;

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
        void Start()
        {
            frame = 0;
            tempHit = null;
            referenceManager.rightControllerScriptAlias.GetComponent<VRTK_StraightPointerRenderer>().enabled = false;
            environmentButtonLayer = LayerMask.NameToLayer("EnvironmentButtonLayer");
            menuLayer = LayerMask.NameToLayer("MenuLayer");
            layerMaskMenu = 1 << menuLayer;
            layerMaskMenu |= 1 << environmentButtonLayer;
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
            Physics.Raycast(origin.position, origin.forward, out hit, 10, layerMaskMenu);
            bool hitMenu = hit.collider != null && hit.collider.gameObject.layer == menuLayer;
            if (hit.collider != null)
            {
                // if we hit anything
                if (controllerModelSwitcher.ActualModel != ControllerModelSwitcher.Model.Menu)
                {
                    controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Menu);
                }

                if (hit.collider.gameObject.layer == menuLayer)
                {
                    // if we hit a button in the menu
                    origin.localRotation = Quaternion.Euler(15f, 0, 0);
                    tempHit = hit.collider.gameObject;
                }
                else if (hit.collider.gameObject.layer == environmentButtonLayer)
                {
                    // if we hit a button in the environment
                    origin.localRotation = Quaternion.Euler(0, 0, 0);
                    referenceManager.multiuserMessageSender.SendMessageMoveLaser(origin, hit.point);
                }
            }
            else
            {
                origin.localRotation = Quaternion.Euler(0, 0, 0);
                referenceManager.multiuserMessageSender.SendMessageMoveLaser(origin, hit.point);
                if (alwaysActive)
                {
                    if (controllerModelSwitcher.DesiredModel != controllerModelSwitcher.ActualModel)
                    {
                        controllerModelSwitcher.ActivateDesiredTool();
                    }
                }
            }

            if (!alwaysActive && !Override)
            {
                // When to switch back to previous model.
                if (!hit.collider)
                {
                    if (controllerModelSwitcher.DesiredModel != controllerModelSwitcher.ActualModel)
                    {
                        controllerModelSwitcher.ActivateDesiredTool();
                    }

                    //referenceManager.rightLaser.enabled = false;
                    referenceManager.rightLaser.tracerVisibility = VRTK_BasePointerRenderer.VisibilityStates.AlwaysOff;
                }
            }
        }


        // Toggle Laser from laser button. Laser should then be active until toggled off.
        public void ToggleLaser(bool active)
        {
            alwaysActive = active;
            if (active)
            {
                referenceManager.rightLaser.tracerVisibility = VRTK_BasePointerRenderer.VisibilityStates.AlwaysOn;
            }
            else
            {
                referenceManager.rightLaser.tracerVisibility = VRTK_BasePointerRenderer.VisibilityStates.AlwaysOff;
            }

            //referenceManager.rightLaser.enabled = active;
            if (controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.TwoLasers)
                referenceManager.leftLaser.tracerVisibility = VRTK_BasePointerRenderer.VisibilityStates.AlwaysOn;
            origin.localRotation = Quaternion.identity;
            RaycastHit laserHit = referenceManager.rightLaser.GetDestinationHit();
            referenceManager.multiuserMessageSender.SendMessageToggleLaser(active, origin, laserHit.point);
        }

        void TouchingObject(bool touch)
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