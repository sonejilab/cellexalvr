using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CellexalVR.General;
using Valve.VR;
using Valve.VR.InteractionSystem;

namespace CellexalVR.AnalysisLogic.H5reader
{
    public class ExpandButtonScript : MonoBehaviour
    {
        public H5ReaderAnnotatorTextBoxScript parentScript;
        private bool controllerInside;
        public Image image;

        private void Start()
        {
            parentScript = GetComponentInParent<H5ReaderAnnotatorTextBoxScript>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Controller"))
            {
                controllerInside = true;
                image.color = Color.red;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Controller"))
            {
                controllerInside = false;
                image.color = Color.white;
            }
        }

        private void Update()
        {
            if (controllerInside && Player.instance.rightHand.grabPinchAction.GetStateDown(Player.instance.rightHand.handType))
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
