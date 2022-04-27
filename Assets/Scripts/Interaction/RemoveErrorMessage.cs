using UnityEngine;
using CellexalVR.General;
namespace CellexalVR.Interaction
{
    /// <summary>
    /// Remove the pop up error message in the scene.
    /// </summary>
    public class RemoveErrorMessage : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject errorMessage;
        // Open XR 
        //private SteamVR_Controller.Device device;
        private UnityEngine.XR.Interaction.Toolkit.ActionBasedController rightController;
        // Open XR 
        //private SteamVR_Controller.Device device;
        private UnityEngine.XR.InputDevice device;
        private bool controllerInside;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            if (!referenceManager)
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
            if (!errorMessage)
            {
                errorMessage = GetComponentInParent<ErrorMessage>().gameObject;
            }
            rightController = referenceManager.rightController;
            CellexalEvents.RightTriggerClick.AddListener(OnTriggerClick);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Smaller Controller Collider"))
            {
                controllerInside = true;
                GetComponent<Renderer>().material.color = Color.red;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.CompareTag("Smaller Controller Collider"))
            {
                controllerInside = false;
                GetComponent<Renderer>().material.color = Color.white;
            }
        }

        private void Update()
        {
        }

        private void OnTriggerClick()
        {
            // Open XR
            //device = SteamVR_Controller.Input((int)rightController.index);
            if (controllerInside)
            {
                Destroy(errorMessage);
            }
        }
    }

}