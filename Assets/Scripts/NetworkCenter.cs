using System;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRTK.GrabAttachMechanics;
using VRTK.SecondaryControllerGrabActions;

/// <summary>
/// This class represents the center of a network. It handles the enlarging when it is pressed.
/// </summary>
public class NetworkCenter : MonoBehaviour
{
    public GameObject replacementPrefab;
    private GameObject pedestal;
    private SteamVR_Controller.Device device;
    private bool controllerInside = false;
    private Vector3 oldLocalPosition;
    private Vector3 oldScale;
    private Quaternion oldRotation;
    private Transform oldParent;
    public bool Enlarged { get; private set; }
    private bool enlarge = false;
    private bool isReplacement = false;
    private NetworkCenter replacing;
    private List<GameObject> arcs = new List<GameObject>();
    private SteamVR_TrackedObject rightController;

    void Start()
    {
        pedestal = GameObject.Find("Pedestal");
        rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
    }

    void FixedUpdate()
    {
        // moving kinematic rigidbodies
        if (enlarge)
        {
            enlarge = false;
            if (!isReplacement)
                EnlargeNetwork();
            else
                BringBackOriginal();
        }
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        // handle input
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            controllerInside = false;
            enlarge = true;
            //controllerInside = false;
            //if (!isReplacement)
            //    EnlargeNetwork();
            //else
            //    BringBackOriginal();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Smaller Controller Collider")
        {
            controllerInside = true;
            //print("controller entered");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Smaller Controller Collider")
        {
            controllerInside = false;
            //print("controller left");
        }
    }

    /// <summary>
    /// Called when the controller is inside the network and the trigger is pressed. Enlarges the network and seperates it from the skeleton and makes it movable by the user.
    /// </summary>
    public void EnlargeNetwork()
    {
        // add a rigidbody and the necessary scripts
        Enlarged = true;
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        var rigidbody = gameObject.GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            rigidbody = gameObject.AddComponent<Rigidbody>();
        }
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
        rigidbody.angularDrag = float.PositiveInfinity;
        var interactableObject = gameObject.AddComponent<VRTK_InteractableObject>();
        interactableObject.isGrabbable = true;
        interactableObject.isUsable = false;
        var grabAttach = gameObject.AddComponent<VRTK_FixedJointGrabAttach>();
        var scalescript = gameObject.AddComponent<VRTK_AxisScaleGrabAction>();
        scalescript.uniformScaling = true;
        interactableObject.grabAttachMechanicScript = grabAttach;
        interactableObject.secondaryGrabActionScript = scalescript;

        grabAttach.precisionGrab = true;
        grabAttach.breakForce = float.PositiveInfinity;

        // save the old variables
        oldParent = transform.parent;
        oldLocalPosition = transform.localPosition;
        oldScale = transform.localScale;
        oldRotation = transform.rotation;

        transform.parent = null;
        transform.position = pedestal.transform.position + new Vector3(0, 1, 0);
        transform.localScale = new Vector3(.7f, .7f, .7f);
        transform.rotation = pedestal.transform.rotation;
        transform.Rotate(-90f, 0, 0);

        // instantiate a replacement in our place
        var replacement = Instantiate(replacementPrefab);
        replacement.transform.parent = oldParent;
        replacement.transform.localPosition = oldLocalPosition;
        replacement.transform.rotation = oldRotation;
        replacement.transform.localScale = oldScale;
        replacement.GetComponent<Renderer>().material.color = GetComponent<Renderer>().material.color;

        // make sure the replacement knows its place in the world
        var replacementScript = replacement.GetComponent<NetworkCenter>();
        replacementScript.isReplacement = true;
        replacementScript.replacing = this;

        // turn on the colliders on the nodes so they can be highlighted
        foreach (BoxCollider b in GetComponentsInChildren<BoxCollider>())
        {
            b.enabled = true;
        }
    }

    /// <summary>
    /// If this network is enlarged, bring it back to the convex hull, if it is a replacement, destroy it and bring back the original 
    /// </summary>
    private void BringBackOriginal()
    {
        if (isReplacement)
        {
            replacing.BringBackOriginal();
            // calling Destroy without the time delay caused the program to crash pretty reliably
            Destroy(GetComponent<Collider>());
            Destroy(GetComponent<Renderer>());
            rightController.gameObject.GetComponentInChildren<VRTK_InteractTouch>().ForceStopTouching();
            gameObject.SetActive(false);
            Destroy(gameObject, .1f);
        }
        else
        {
            Enlarged = false;
            // this network will now be part of the convex hull which already has a rigidbody and these scripts
            Destroy(gameObject.GetComponent<VRTK_FixedJointGrabAttach>());
            Destroy(gameObject.GetComponent<VRTK_AxisScaleGrabAction>());
            Destroy(gameObject.GetComponent<VRTK_InteractableObject>());
            Destroy(gameObject.GetComponent<Rigidbody>());
            //var interactableObject = gameObject.GetComponent<VRTK_InteractableObject>();
            //interactableObject.isGrabbable = false;
            //interactableObject.isUsable = true;
            GetComponent<Renderer>().enabled = true;
            GetComponent<Collider>().enabled = true;
            transform.parent = oldParent;
            transform.localPosition = oldLocalPosition;
            transform.rotation = oldRotation;
            transform.localScale = oldScale;
            ////var rigidbody = gameObject.GetComponentInParent<Rigidbody>();
            //var rigidbody2 = rigidbody.gameObject.GetComponentInParent<Rigidbody>();
            //rigidbody2.transform.Translate(.1f, 0, 0);
            //print(oldParent.name);
            //oldParent.Translate(0, 0, 0);
            //rigidbody.transform.Translate(-1, 0, 0);
            //print(GetComponent<Collider>().bounds.center);
            foreach (BoxCollider b in GetComponentsInChildren<BoxCollider>())
            {
                b.enabled = false;
            }
        }
    }

    internal void AddArc(GameObject edge)
    {
        arcs.Add(edge);
    }

    public void SetArcsVisible(bool toggleToState)
    {
        foreach (GameObject arc in arcs)
        {
            arc.SetActive(toggleToState);
        }
    }
}
