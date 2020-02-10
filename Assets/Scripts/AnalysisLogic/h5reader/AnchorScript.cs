using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CellexalVR.General;

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
    public string type;
    [SerializeField]private bool isAttachedToHand = false;

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
            
            if (!rightController.GetComponentInChildren<AnchorScript>() && !isAnchorA) //Press to move anchor B
            {
                print(1);
                transform.parent = rightController.transform;
                transform.position = rightController.transform.position;
                isAttachedToHand = true;
            }
            else if (expandButtonScript && isAttachedToHand && !isAnchorA) //If inside an expandButton and its attached to the hand let it go
            {
                print(2);
                transform.parent = expandButtonScript.transform;
                isAttachedToHand = false;
                if (type == "coords")
                {
                    anchorA.GetComponentInParent<ProjectionObjectScript>().coordsPath = expandButtonScript.parentScript.getPath();
                }
                else if (type == "velocity")
                {
                    anchorA.GetComponentInParent<ProjectionObjectScript>().velocityPath = expandButtonScript.parentScript.getPath();
                }
                else if (type == "cell_names")
                {
                    anchorA.GetComponentInParent<h5readerAnnotater>().config.Add("cellnames", expandButtonScript.parentScript.getPath());
                }
                else if (type == "gene_names")
                {
                    anchorA.GetComponentInParent<h5readerAnnotater>().config.Add("genenames", expandButtonScript.parentScript.getPath());
                }
                else if (type == "gene_expressions")
                {
                    anchorA.GetComponentInParent<h5readerAnnotater>().config.Add("cellexpr", expandButtonScript.parentScript.getPath());
                }
                else if (type == "cell_expressions")
                {
                    anchorA.GetComponentInParent<h5readerAnnotater>().config.Add("attr_", expandButtonScript.parentScript.getPath());
                }
            }
            else if(isAttachedToHand && !isAnchorA) //Pressing in free space return it
            {
                print(3);
                transform.parent = anchorA.transform.parent;
                transform.localPosition = anchorA.transform.localPosition;
                isAttachedToHand = false;
            }else if(isAnchorA && line.isExpanded())
            {
                print(4);
                LineScript newLine = line.addLine();
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
