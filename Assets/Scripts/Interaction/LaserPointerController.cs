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
        //private int layerMaskGraph;
        //private int layerMaskNetwork;
        //private int layerMaskOther;
        private bool alwaysActive;
        private bool keyboardActive;
        private bool hitLastFrame;
        private ControllerModelSwitcher controllerModelSwitcher;

        public ReferenceManager referenceManager;
        public GameObject panel;
        public bool Override { get; set; }
        // Use this for initialization
        void Start()
        {
            frame = 0;
            tempHit = null;
            GetComponent<VRTK_StraightPointerRenderer>().enabled = false;
            layerMaskMenu = 1 << LayerMask.NameToLayer("MenuLayer");
            controllerModelSwitcher = referenceManager.controllerModelSwitcher;

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
            transform.localRotation = Quaternion.Euler(15f, 0, 0);
            Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 10, layerMaskMenu);
            bool hitSomething = hit.collider != null;
            if (hitSomething)
            {
                tempHit = hit.collider.gameObject;
                if (controllerModelSwitcher.ActualModel != ControllerModelSwitcher.Model.Menu)
                {
                    //controllerModelSwitcher.TurnOffActiveTool(true);
                    controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Menu);
                    panel.transform.localPosition = new Vector3(0.0f, 0.0f, 0.01f);
                    panel.transform.localRotation = Quaternion.Euler(-15, 5, 1);
                }
            }
            else
            {
                transform.localEulerAngles = new Vector3(0f, 0f, 0f);
            }
            if (!hitSomething && alwaysActive)
            {
                transform.localRotation = Quaternion.Euler(0, 0, 0);
                //Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, 10, layerMaskKeyboard);
                //if (hit.collider && (hit.collider.gameObject.CompareTag("Keyboard") || hit.collider.gameObject.CompareTag("PreviousSearchesListNode")))
                //{
                //    controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Keyboard);
                //}
                //else
                //{
                controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.TwoLasers);
                panel.transform.localPosition = new Vector3(0, 0, 0);
                panel.transform.localRotation = Quaternion.Euler(0, 5, 1);
                //}
            }
            if (!alwaysActive && !Override)
            {
                // When to switch back to previous model. 
                if (!hit.collider)
                {
                    if (controllerModelSwitcher.DesiredModel != controllerModelSwitcher.ActualModel)
                    {
                        controllerModelSwitcher.ActivateDesiredTool();
                        panel.transform.localPosition = new Vector3(0, 0, 0);
                        panel.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    }
                    GetComponent<VRTK_StraightPointerRenderer>().enabled = false;

                }
            }
            //if (!hitSomething && hitLastFrame)
            //{
            //    print("Unhighlight");
            //    UnhighlightAllButtons();
            //}

            //hitLastFrame = hitSomething;
        }

        //private void UnhighlightAllButtons()
        //{
        //    foreach (var button in referenceManager.mainMenu.GetComponentsInChildren<CellexalButton>())
        //    {
        //        button.SetHighlighted(false);
        //    }
        //}

        // Toggle Laser from laser button. Laser should then be active until toggled off.
        public void ToggleLaser(bool active)
        {
            alwaysActive = active;
            GetComponent<VRTK_StraightPointerRenderer>().enabled = alwaysActive;
            transform.localRotation = Quaternion.identity;
            //if (alwaysActive)
            //{
            //    layerMask = layerMaskMenu | layerMaskKeyboard | layerMaskGraph | layerMaskNetwork;
            //}
            //if (!alwaysActive)
            //{
            //    layerMask = layerMaskMenu;
            //}
        }
    }
}