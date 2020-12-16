using CellexalVR.General;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace CellexalVR.Filters
{

    public class FilterCreatorSaveArea : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        public GameObject leftBorder;
        public GameObject rightBorder;
        public GameObject bottomBorder;

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
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                controllerInside = true;
                leftBorder.GetComponent<Renderer>().material.color = Color.green;
                rightBorder.GetComponent<Renderer>().material.color = Color.green;
                bottomBorder.GetComponent<Renderer>().material.color = Color.green;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                controllerInside = false;
                leftBorder.GetComponent<Renderer>().material.color = originalColor;
                rightBorder.GetComponent<Renderer>().material.color = originalColor;
                bottomBorder.GetComponent<Renderer>().material.color = originalColor;
            }
        }

        private void Update()
        {
            if (controllerInside && Player.instance.rightHand.grabPinchAction.GetStateDown(Player.instance.rightHand.handType))
            {
                referenceManager.filterManager.SaveFilter();
                // Save filter
            }
        }

    }
}
