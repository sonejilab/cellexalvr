using CellexalVR.General;
using CellexalVR.Menu.Buttons;
using UnityEngine;
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
        private bool alwaysActive;
        private bool keyboardActive;
        private bool touchingObject;
        private bool hitLastFrame;
        private ControllerModelSwitcher controllerModelSwitcher;

        public ReferenceManager referenceManager;
        public Transform origin;
        public bool Override { get; set; }

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
            layerMaskMenu = 1 << LayerMask.NameToLayer("MenuLayer");
            controllerModelSwitcher = referenceManager.controllerModelSwitcher;
            //CellexalEvents.ObjectGrabbed.AddListener(() => TouchingObject(true));
            //CellexalEvents.ObjectUngrabbed.AddListener(() => TouchingObject(false));

        }
        private void Update()
        {
            frame++;
            if (frame >= 5)
            {
                frame = 0;
                Frame5Update();
            }
        }

        // Call every 3rd frame.
        private void Frame5Update()
        {
            RaycastHit hit;
            origin.localRotation = Quaternion.Euler(15f, 0, 0);
            Physics.Raycast(origin.position, origin.forward, out hit, 10, layerMaskMenu);
            bool hitMenu = hit.collider != null;
            if (hitMenu)
            {
                tempHit = hit.collider.gameObject;
                if (controllerModelSwitcher.ActualModel != ControllerModelSwitcher.Model.Menu)
                {
                    //controllerModelSwitcher.TurnOffActiveTool(true);
                    controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Menu);
                }
            }
            else
            {
                origin.localEulerAngles = new Vector3(0f, 0f, 0f);
            }

            if (!hitMenu && alwaysActive)
            {
                origin.localRotation = Quaternion.Euler(0, 0, 0);


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