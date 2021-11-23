using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CellexalVR.General;
using CellexalVR.Interaction;

namespace CellexalVR.AnalysisLogic.H5reader
{
    public class ExpandButtonScript : MonoBehaviour
    {
        private ReferenceManager referenceManager;
        public H5ReaderAnnotatorTextBoxScript parentScript;
        // Open XR 
		//private SteamVR_Controller.Device device;
		private UnityEngine.XR.Interaction.Toolkit.ActionBasedController rightController;
        // Open XR 
		//private SteamVR_Controller.Device device;
		private UnityEngine.XR.InputDevice device;
        private bool controllerInside;
        public Image image;

        // Start is called before the first frame update
        void Start()
        {
            if (!referenceManager)
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
            rightController = referenceManager.rightController;
            parentScript = GetComponentInParent<H5ReaderAnnotatorTextBoxScript>();

            CellexalEvents.RightTriggerClick.AddListener(OnTriggerPressed);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
            {
                controllerInside = true;
                image.color = Color.red;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
            {
                controllerInside = false;
                image.color = Color.white;
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
                if (!parentScript.isBottom)
                {
                    foreach (H5ReaderAnnotatorTextBoxScript key in parentScript.subkeys.Values)
                    {
                        key.gameObject.SetActive(!key.gameObject.activeSelf);
                    }
                    H5ReaderAnnotatorTextBoxScript parent = parentScript;
                    while (!parent.isTop)
                    {
                        parent = parent.transform.parent.GetComponent<H5ReaderAnnotatorTextBoxScript>();
                    }
                    float contentSize = parent.UpdatePosition(10f);
                    parent.GetComponentInParent<H5readerAnnotater>().display.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, contentSize);
                }
            }

        }
    }
}
