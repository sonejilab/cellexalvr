using CellexalVR.General;
using UnityEngine;
using Valve.VR;

namespace CellexalVR.SceneObjects
{

    public class Lever : MonoBehaviour
    {
        public ReferenceManager referenceManager;

        private bool controllerInside = false;
        private SteamVR_Behaviour_Pose rightController;
        // private SteamVR_Controller.Device device;
        private bool animateDown = false;
        private bool animateUp = false;
        private Renderer rend;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            rightController = referenceManager.rightController;
            rend = GetComponent<Renderer>();
        }

        private void Update()
        {
            // device = SteamVR_Controller.Input((int)rightController.index);
            if (controllerInside)// && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {
                animateDown = true;
            }

            if (animateDown)
            {
                transform.parent.Rotate(0, 0, 30f * Time.deltaTime);
                if (transform.parent.localRotation.eulerAngles.z >= 40)
                {
                    animateDown = false;
                    animateUp = true;
                    // multiple_exp PullLever();
                }
            }
            if (animateUp)
            {
                transform.parent.Rotate(0, 0, -30f * Time.deltaTime);
                if (transform.parent.localRotation.eulerAngles.z <= 5)
                {
                    animateUp = false;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Controller"))
            {
                controllerInside = true;
                Color color = rend.material.color;
                color.a = 1;
                rend.material.color = color;

            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Controller"))
            {
                controllerInside = false;
                Color color = rend.material.color;
                color.a = 0.3f;
                rend.material.color = color;
            }
        }

        private void OnDisable()
        {
            controllerInside = false;
        }

        // multiple_exp   public void PullLever()
        // multiple_exp   {
        // multiple_exp       referenceManager.loaderController.LoadAllCells();
        // multiple_exp   }
    }
}
