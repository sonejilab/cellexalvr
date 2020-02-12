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

            if (!rightController.GetComponentInChildren<AnchorScript>() && !isAnchorA && !isAttachedToHand) //Press to move anchor B
            {
                print(1);
                transform.parent = rightController.transform;
                transform.position = rightController.transform.position;
                isAttachedToHand = true;

                if (expandButtonScript) //if moving away from expandButton
                {
                    Dictionary<string, string> config = anchorA.GetComponentInParent<h5readerAnnotater>().config;
                    string rem = "";
                    switch (line.type)
                    {
                        case "cell_names": rem = "cellnames"; break;
                        case "gene_names": rem = "genenames"; break;
                        case "gene_expressions": rem = "geneexpr"; break;
                        case "cell_expressions": rem = "attr_" + expandButtonScript.parentScript.name; break;
                    }
                    config.Remove(rem);
                    ProjectionObjectScript projectionObjectScript = anchorA.GetComponentInParent<ProjectionObjectScript>();
                    if (projectionObjectScript && projectionObjectScript.paths.ContainsKey(line.type))
                    {
                            projectionObjectScript.paths.Remove(line.type);
                    }

                }
            }
            else if (expandButtonScript && isAttachedToHand && !isAnchorA) //If inside an expandButton and its attached to the hand let it go
            {
                print(2);
                transform.parent = expandButtonScript.transform;
                transform.localPosition = Vector3.zero;
                isAttachedToHand = false;
                string path = expandButtonScript.parentScript.getPath();
                ProjectionObjectScript projectionObjectScript = anchorA.GetComponentInParent<ProjectionObjectScript>();
                if (projectionObjectScript)
                {
                    projectionObjectScript.paths.Add(line.type, path);
                    if(line.type == "X")
                    {
                        int start = path.LastIndexOf('/');
                        print(path);
                        print(start);
                        string[] names = path.Substring(start).Split('_');

                        anchorA.GetComponentInParent<ProjectionObjectScript>().changeName(names[names.Length - 1]);
                    }
                }
                else
                {
                    Dictionary<string, string> config = anchorA.GetComponentInParent<h5readerAnnotater>().config;
                    if (line.type == "cell_names")
                    {
                        if (config.ContainsKey("cellnames"))
                        {
                            config["cellnames"] = expandButtonScript.parentScript.getPath();
                        }
                        else
                        {
                            config.Add("cellnames", expandButtonScript.parentScript.getPath());
                        }
                    }
                    else if (line.type == "gene_names")
                    {
                        if (config.ContainsKey("cellnames"))
                        {
                            config["genenames"] = expandButtonScript.parentScript.getPath();
                        }
                        else
                        {
                            config.Add("genenames", expandButtonScript.parentScript.getPath());
                        }
                    }
                    else if (line.type == "gene_expressions")
                    {
                        if (config.ContainsKey("cellnames"))
                        {
                            config["geneexpr"] = expandButtonScript.parentScript.getPath();
                        }
                        else
                        {
                            config.Add("geneexpr", expandButtonScript.parentScript.getPath());
                        }
                    }
                    else if (line.type == "cell_expressions")
                    {
                        print("attr_" + expandButtonScript.parentScript.name);
                        config.Add("attr_" + expandButtonScript.parentScript.name, expandButtonScript.parentScript.getPath());
                    }
                }

            }
            else if (isAttachedToHand && !isAnchorA) //Pressing in free space return it
            {
                print(3);
                transform.parent = anchorA.transform.parent;
                transform.localPosition = anchorA.transform.localPosition;
                isAttachedToHand = false;
            } else if (isAnchorA && line.isExpanded() && line.isMulti)
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
