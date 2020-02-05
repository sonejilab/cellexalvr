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
    public LineScript line;
    public bool isAnchorA;
    public AnchorScript otherAnchor;

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
            if (!rightController.GetComponentInChildren<AnchorScript>() && isAnchorA)
            {
                otherAnchor.transform.parent = rightController.transform;
                otherAnchor.transform.position = rightController.transform.position;
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
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
        {
            controllerInside = false;
            GetComponent<Renderer>().material.color = Color.white;
        }
    }
}
