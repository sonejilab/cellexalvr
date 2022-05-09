using CellexalVR.General;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

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
        [SerializeField] private GameObject rayEndPoint;

        private ControllerModelSwitcher controllerModelSwitcher;

        public ReferenceManager referenceManager;
        public XRRayInteractor rightLaser;
        public XRRayInteractor leftLaser;
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
            GetComponentInChildren<XRRayInteractor>().enabled = false;
            environmentButtonLayer = LayerMask.NameToLayer("EnvironmentButtonLayer");
            keyboardLayer = LayerMask.NameToLayer("KeyboardLayer");
            menuLayer = LayerMask.NameToLayer("MenuLayer");
            layerMaskMenu = 1 << menuLayer;
            layerMaskEnv = 1 << environmentButtonLayer | 1 << keyboardLayer;
            controllerModelSwitcher = referenceManager.controllerModelSwitcher;
            var rightInteractor = referenceManager.rightController.gameObject.GetComponent<XRDirectInteractor>();
            var leftInteractor = referenceManager.leftController.gameObject.GetComponent<XRDirectInteractor>();
            rightInteractor.selectEntered.AddListener(ToggleRightLaserInteractorOff);
            rightInteractor.selectExited.AddListener(ToggleRightLaserInteractorOn);
            leftInteractor.selectEntered.AddListener(ToggleLeftLaserInteractorOff);
            leftInteractor.selectExited.AddListener(ToggleLeftLaserInteractorOn);

        }

        private void OnDisable()
        {
            rayEndPoint.SetActive(false);
        }

        private void OnEnable()
        {
            rayEndPoint.SetActive(true);
        }

        private void ToggleRightLaserInteractorOn(SelectExitEventArgs args)
        {
            leftLaser.interactionLayers = LayerMask.NameToLayer("Everything");
        }

        private void ToggleRightLaserInteractorOff(SelectEnterEventArgs args)
        {
            rightLaser.interactionLayers = LayerMask.NameToLayer("Nothing");
        }

        private void ToggleLeftLaserInteractorOn(SelectExitEventArgs args)
        {
            leftLaser.interactionLayers = LayerMask.NameToLayer("Everything");
        }

        private void ToggleLeftLaserInteractorOff(SelectEnterEventArgs args)
        {
            leftLaser.interactionLayers = LayerMask.NameToLayer("Nothing");
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
            if (hit.collider)
            {
                // if we hit a button in the menu
                ToggleLaser(true);
                if (controllerModelSwitcher.ActualModel != ControllerModelSwitcher.Model.Menu)
                {
                    controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Menu);
                }
                return;
            }
            Physics.Raycast(origin.position, origin.forward, out hit, 10, layerMaskEnv);
            if (hit.collider &&
                controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.Normal ||
                controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.Keyboard ||
                controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.TwoLasers)

            {
                MultiUserToggle(true);
                ToggleLaser(true);
                Vector3 hitPoint;
                if (!hit.collider)
                {
                    hitPoint = origin.position + (origin.forward * 10);
                }
                else
                {
                    hitPoint = hit.point;
                }
                MultiUserMove(origin, hitPoint);
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
                MultiUserMove(origin, hitPoint);
                if (controllerModelSwitcher.DesiredModel != controllerModelSwitcher.ActualModel)
                {
                    controllerModelSwitcher.ActivateDesiredTool();
                }
            }
            else if (referenceManager.rightLaser.enabled &&
                     controllerModelSwitcher.ActualModel != ControllerModelSwitcher.Model.Keyboard)
            {
                ToggleLaser(false);
                MultiUserToggle(false);
            }

            if (alwaysActive || Override) return;
            if (hit.collider) return;
            if (controllerModelSwitcher.DesiredModel != controllerModelSwitcher.ActualModel)
            {
                controllerModelSwitcher.ActivateDesiredTool();
            }
        }

        private void MultiUserToggle(bool toggle)
        {
            if (referenceManager.multiuserMessageSender != null)
            {
                referenceManager.multiuserMessageSender.SendMessageToggleLaser(toggle);
            }
        }

        private void MultiUserMove(Transform origin, Vector3 hitPoint)
        {
            if (referenceManager.multiuserMessageSender != null)
            {
                referenceManager.multiuserMessageSender.SendMessageMoveLaser(origin, hitPoint);
            }
        }

        // Toggle Laser from laser button. Laser should then be active until toggled off.
        public void ToggleLaser(bool active)
        {
            if (active == referenceManager.rightLaser.enabled)
                return;
            // OpenXR
            referenceManager.rightLaser.enabled = active;
            if (controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.TwoLasers)
                referenceManager.leftLaser.enabled = true;
            MultiUserToggle(active);
            rayEndPoint.SetActive(active);
        }

        private void TouchingObject(bool touch)
        {
            referenceManager.rightLaser.enabled = !touch;
            if (controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.TwoLasers)
            {
                referenceManager.leftLaser.enabled = !touch;
            }

        }
    }
}