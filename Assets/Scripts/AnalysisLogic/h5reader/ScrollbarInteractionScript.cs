using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CellexalVR.General;
using UnityEngine.UI;
namespace CellexalVR.Interaction
{
    public class ScrollbarInteractionScript : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        private SteamVR_TrackedObject rightController;
        private SteamVR_Controller.Device device;
        private bool controllerInside;
        public RectTransform rect;
        public Scrollbar scrollbar;

        // Start is called before the first frame update
        private void Start()
        {
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
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
            {
                controllerInside = false;
            }
        }

        private void Update()
        {
            device = SteamVR_Controller.Input((int)rightController.index);
            if (controllerInside && device.GetPress(SteamVR_Controller.ButtonMask.Trigger))
            {
                float y = transform.InverseTransformPoint(device.transform.pos).y;
                float height = rect.rect.height;
                float temp = Mathf.Clamp01(y / height + 0.5f);
                scrollbar.value = temp;
            }
        }
    }
}


