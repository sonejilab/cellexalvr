using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CellexalVR.General;
namespace CellexalVR.AnalysisLogic.H5reader
{
    public class AnchorScript : MonoBehaviour
    {

        BoxCollider boxCollider;

        public ReferenceManager referenceManager;
        private SteamVR_TrackedObject rightController;
        private SteamVR_Controller.Device device;
        private bool controllerInside;

        public bool isAnchorA;
        public AnchorScript anchorA;
        public AnchorScript anchorB;
        public LineScript line;

        public ExpandButtonScript expandButtonScript;
        [SerializeField] private bool isAttachedToHand = false;

        // Start is called before the first frame update
        void Start()
        {
            if (!referenceManager)
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
            rightController = referenceManager.rightController;
        }

        // Update is called once per frame
        void Update()
        {
            device = SteamVR_Controller.Input((int)rightController.index);
            if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
            {

                if (!rightController.GetComponentInChildren<AnchorScript>() && !isAnchorA && !isAttachedToHand) //Press to move anchor B
                {
                    print(1);
                    transform.parent = rightController.transform;
                    transform.position = rightController.transform.position;
                    isAttachedToHand = true;

                    if (expandButtonScript) //if moving away from expandButton
                    {
                        ProjectionObjectScript projectionObjectScript = anchorA.GetComponentInParent<ProjectionObjectScript>();
                        if (projectionObjectScript)
                        {
                            projectionObjectScript.RemoveFromPaths(line.type);
                        }
                        else
                        {
                            H5readerAnnotater h5ReaderAnnotater = anchorA.GetComponentInParent<H5readerAnnotater>();

                            if (line.type == "attrs")
                            {
                                h5ReaderAnnotater.RemoveFromConfig("attr_" + expandButtonScript.parentScript.name);
                            }
                            else
                            {
                                h5ReaderAnnotater.RemoveFromConfig(line.type);
                            }
                        }

                    }
                }
                else if (expandButtonScript && isAttachedToHand && !isAnchorA) //If inside an expandButton and its attached to the hand let it go
                {
                    print(2);
                    transform.parent = expandButtonScript.transform;
                    transform.localPosition = Vector3.zero;
                    isAttachedToHand = false;

                    string path = expandButtonScript.parentScript.GetPath();
                    char dataType = expandButtonScript.parentScript.GetDataType();

                    int start = path.LastIndexOf('/');
                    string name;
                    if (start != -1)
                        name = path.Substring(start);
                    else
                        name = path;

                    ProjectionObjectScript projectionObjectScript = anchorA.GetComponentInParent<ProjectionObjectScript>();
                    if (projectionObjectScript)
                    {
                        if (line.type == "X")
                        {
                            anchorA.GetComponentInParent<ProjectionObjectScript>().ChangeName(name);
                        }
                        projectionObjectScript.AddToPaths(line.type, path, dataType);
                        
                    }
                    else
                    {
                        H5readerAnnotater h5ReaderAnnotater = anchorA.GetComponentInParent<H5readerAnnotater>();
                        if (line.type == "attrs")
                        {
                            h5ReaderAnnotater.AddToConfig("attr_" + name, path, dataType);
                        }
                        else
                        {
                            h5ReaderAnnotater.AddToConfig(line.type, path, dataType);
                        }

                    }

                }
                else if (isAttachedToHand && !isAnchorA) //Pressing in free space return it
                {
                    print(3);
                    transform.parent = anchorA.transform.parent;
                    transform.localPosition = anchorA.transform.localPosition;
                    isAttachedToHand = false;
                }
                else if (isAnchorA && line.IsExpanded() && line.isMulti)
                {
                    print(4);
                    LineScript newLine = line.AddLine();
                    newLine.AnchorB.transform.parent = rightController.transform;
                    newLine.AnchorB.transform.position = rightController.transform.position;
                    newLine.AnchorB.isAttachedToHand = true;
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {

            if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
            {
                controllerInside = true;
                GetComponent<Renderer>().material.color = Color.yellow;
            }
            ExpandButtonScript ebs = other.gameObject.GetComponent<ExpandButtonScript>();
            if (ebs && ebs.parentScript.isBottom)
            {
                expandButtonScript = ebs;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
            {
                controllerInside = false;
                GetComponent<Renderer>().material.color = Color.white;
            }
            ExpandButtonScript ebs = other.gameObject.GetComponent<ExpandButtonScript>();
            if (ebs == expandButtonScript)
            {
                expandButtonScript = null;
            }
        }
    }
}
