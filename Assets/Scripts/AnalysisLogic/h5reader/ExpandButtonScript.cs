using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CellexalVR.General;
namespace CellexalVR.AnalysisLogic.H5reader
{
    public class ExpandButtonScript : MonoBehaviour
    {
        private ReferenceManager referenceManager;
        public H5ReaderAnnotatorTextBoxScript parentScript;
        private SteamVR_TrackedObject rightController;
        private SteamVR_Controller.Device device;
        private bool controllerInside;
        public Image image;
        public bool isExpanded;
        public Sprite plusImage;
        public Sprite minusImage;
        public Sprite circleImage;
        public bool anchorInside;
        public AnchorScript anchor;

        // Start is called before the first frame update
        void Start()
        {
            if (!referenceManager)
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
            rightController = referenceManager.rightController;
            parentScript = GetComponentInParent<H5ReaderAnnotatorTextBoxScript>();
            if (parentScript.isBottom)
            {
                image.sprite = circleImage;
            }
            else
            {
                image.sprite = plusImage;
                isExpanded = false;
            }

        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
            {
                controllerInside = true;
                image.color = Color.yellow;
            }
            if (other.gameObject.name.Equals("AnchorB") && anchor == null)
            {
                anchor = other.gameObject.GetComponent<AnchorScript>();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
            {
                controllerInside = false;
                image.color = Color.white;
            }
            if (anchor && other.gameObject == anchor.gameObject)
            {
                anchor = null;
            }
        }

        public void pressButton()
        {
            AnchorScript a = GetComponentInChildren<AnchorScript>();
            if (!parentScript.isBottom)
            {
                isExpanded = !isExpanded;
                if (isExpanded)
                {
                    print("setting minus");
                    image.sprite = minusImage;
                }
                else
                {
                    image.sprite = plusImage;
                }

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
            else if (a)
            {
                a.transform.parent = rightController.transform;
                a.transform.position = rightController.transform.position;
                a.isAttachedToHand = true;
                ProjectionObjectScript projectionObjectScript = a.anchorA.GetComponentInParent<ProjectionObjectScript>();
                if (projectionObjectScript)
                {
                    projectionObjectScript.RemoveFromPaths(a.line.type);
                }
                else
                {
                    H5readerAnnotater h5ReaderAnnotater = a.anchorA.GetComponentInParent<H5readerAnnotater>();

                    if (a.line.type == "attrs")
                    {
                        h5ReaderAnnotater.RemoveFromConfig("attr_" + parentScript.name);
                    }
                    else
                    {
                        h5ReaderAnnotater.RemoveFromConfig(a.line.type);
                    }
                }
            }else if (anchor)
            {
                anchor.transform.parent = transform;
                anchor.transform.localPosition = Vector3.zero;
                anchor.isAttachedToHand = false;

                string path = parentScript.GetPath();
                char dataType = parentScript.GetDataType();

                int start = path.LastIndexOf('/');
                string name;
                if (start != -1)
                    name = path.Substring(start);
                else
                    name = path;

                ProjectionObjectScript projectionObjectScript = anchor.anchorA.GetComponentInParent<ProjectionObjectScript>();
                if (projectionObjectScript)
                {
                    if (anchor.line.type == "X")
                    {
                        anchor.anchorA.GetComponentInParent<ProjectionObjectScript>().ChangeName(name);
                    }
                    projectionObjectScript.AddToPaths(anchor.line.type, path, dataType);

                }
                else
                {
                    H5readerAnnotater h5ReaderAnnotater = anchor.anchorA.GetComponentInParent<H5readerAnnotater>();
                    if (anchor.line.type == "attrs")
                    {
                        h5ReaderAnnotater.AddToConfig("attr_" + name, path, dataType);
                    }
                    else
                    {
                        h5ReaderAnnotater.AddToConfig(anchor.line.type, path, dataType);
                    }

                }
            }
        }

        private void Update()
        {

        }
    }
}
