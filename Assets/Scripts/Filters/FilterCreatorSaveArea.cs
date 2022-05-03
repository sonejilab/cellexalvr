using CellexalVR.General;
using CellexalVR.Interaction;
using UnityEngine;

namespace CellexalVR.Filters
{

    public class FilterCreatorSaveArea : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject leftBorder;
        public GameObject rightBorder;
        public GameObject bottomBorder;

        // Open XR 
        //private SteamVR_Controller.Device device;
        private UnityEngine.XR.Interaction.Toolkit.ActionBasedController rightController;
        // Open XR 
        //private SteamVR_Controller.Device device;
        private UnityEngine.XR.InputDevice device;
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
            CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Smaller Controller Collider"))
            {
                controllerInside = true;
                leftBorder.GetComponent<Renderer>().material.color = Color.green;
                rightBorder.GetComponent<Renderer>().material.color = Color.green;
                bottomBorder.GetComponent<Renderer>().material.color = Color.green;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Smaller Controller Collider"))
            {
                controllerInside = false;
                leftBorder.GetComponent<Renderer>().material.color = originalColor;
                rightBorder.GetComponent<Renderer>().material.color = originalColor;
                bottomBorder.GetComponent<Renderer>().material.color = originalColor;
            }
        }

        private void OnTriggerClick()
        {
            // Open XR
            //device = SteamVR_Controller.Input((int)rightController.index);
            if (controllerInside)
            {
                referenceManager.filterManager.SaveFilter();
                // Save filter
            }
        }
    }
}
