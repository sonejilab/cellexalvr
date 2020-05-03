using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CellexalVR.General;
using System.IO;
using UnityEngine.UI;
namespace CellexalVR.AnalysisLogic.H5reader
{
    public class ButtonPresser : MonoBehaviour
    {
        // Start is called before the first frame update
        public BoxCollider collider;
        public ReferenceManager referenceManager;
        private SteamVR_TrackedObject rightController;
        private SteamVR_Controller.Device device;
        private bool controllerInside;
        private Button button;

        void Start()
        {
            button = GetComponent<Button>();
            if (!referenceManager)
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
            rightController = referenceManager.rightController;

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
            {
                controllerInside = true;
                if(button.transition == Button.Transition.ColorTint)
                    button.targetGraphic.color = button.colors.highlightedColor;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
            {
                controllerInside = false;
                if (button.transition == Button.Transition.ColorTint)
                    button.targetGraphic.color = button.colors.normalColor;
            }
        }

        private void Update()
        {
            device = SteamVR_Controller.Input((int)rightController.index);
            if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                button.onClick.Invoke();
                
            }
        }
    }
}