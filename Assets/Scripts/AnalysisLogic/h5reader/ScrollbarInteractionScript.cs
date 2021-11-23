using CellexalVR.General;
using CellexalVR.Interaction;
using UnityEngine;
using UnityEngine.UI;
namespace CellexalVR.AnalysisLogic.H5reader
{
    public class ScrollbarInteractionScript : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        // Open XR 
		//private SteamVR_Controller.Device device;
		private UnityEngine.XR.Interaction.Toolkit.ActionBasedController rightController;
        // Open XR 
		//private SteamVR_Controller.Device device;
		private UnityEngine.XR.InputDevice device;
        private bool controllerInside;
        public RectTransform handleRect;
        public RectTransform slidingArea;
        public Scrollbar scrollbar;
        public BoxCollider boxCollider;

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
            // Open XR
            //device = SteamVR_Controller.Input((int)rightController.index);
            //device = DeviceManager.instance.GetDevice(UnityEngine.XR.XRNode.RightHand);
            
            //if (controllerInside && DeviceManager.instance.GetTrigger(device) == 2)
            //{
            //    float temp;
            //    //Horizontal
            //    if (slidingArea.rect.height > slidingArea.rect.width)
            //    {
            //        float y = slidingArea.transform.InverseTransformPoint(device.transform.pos).y;
            //        float height = slidingArea.rect.height;
            //        temp = Mathf.Clamp01(y / height + 0.5f);
            //    }
            //    else
            //    {
            //        float x = slidingArea.transform.InverseTransformPoint(device.transform.pos).x;
            //        float width = slidingArea.rect.width;
            //        temp = Mathf.Clamp01(x / width + 0.5f);
            //    }


            //    scrollbar.value = temp;
            //}
            //Vector3 size = handleRect.rect.size;
            //size.z = 10;
            //boxCollider.size = size;


        }

    }
}


