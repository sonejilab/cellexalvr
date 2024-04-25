using CellexalVR.General;
using UnityEngine;
using UnityEngine.UI;

namespace CellexalVR.AnalysisLogic.H5reader
{
    public class ButtonPresser : MonoBehaviour
    {
        // Start is called before the first frame update
        public new BoxCollider collider;
        public ReferenceManager referenceManager;
        [SerializeField] private GameObject note;
        // Open XR 
        //private SteamVR_Controller.Device device;
        private UnityEngine.XR.Interaction.Toolkit.ActionBasedController rightController;
        // Open XR 
        //private SteamVR_Controller.Device device;
        private UnityEngine.XR.InputDevice device;
        private bool controllerInside;
        [SerializeField] private Color color;
        private Button button;

        void Start()
        {
            button = GetComponent<Button>();
            if (!referenceManager)
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
            rightController = referenceManager.rightController;
            collider.size = new Vector3(70, 30, 1);
            collider.center = new Vector3(0, -15, 0);

            CellexalEvents.RightTriggerClick.AddListener(OnTriggerPressed);
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
        }

        private void OnTriggerPressed()
        {
            // Open XR
            if (controllerInside)
            {
                button.onClick.Invoke();

            }
        }
    }
}