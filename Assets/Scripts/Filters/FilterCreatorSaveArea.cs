using CellexalVR.General;
using UnityEngine;

namespace CellexalVR.Filters
{

    public class FilterCreatorSaveArea : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject leftBorder;
        public GameObject rightBorder;
        public GameObject bottomBorder;

        private SteamVR_TrackedObject rightController;
        private SteamVR_Controller.Device device;
        private bool controllerInside;
        private Color originalColor;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            originalColor = leftBorder.GetComponent<Renderer>().material.color;
            rightController = referenceManager.rightController;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
            {
                controllerInside = true;
                leftBorder.GetComponent<Renderer>().material.color = Color.green;
                rightBorder.GetComponent<Renderer>().material.color = Color.green;
                bottomBorder.GetComponent<Renderer>().material.color = Color.green;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
            {
                controllerInside = false;
                leftBorder.GetComponent<Renderer>().material.color = originalColor;
                rightBorder.GetComponent<Renderer>().material.color = originalColor;
                bottomBorder.GetComponent<Renderer>().material.color = originalColor;
            }
        }

        private void Update()
        {
            device = SteamVR_Controller.Input((int)rightController.index);
            if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                referenceManager.filterManager.SaveFilter();
                // Save filter
            }
        }

    }
}
