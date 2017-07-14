﻿using UnityEngine;
using VRTK;
using VRTK.GrabAttachMechanics;

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
    private bool isReplacement = false;
    private NetworkCenter replacing;

    void Start()
    {
        pedestal = GameObject.Find("Pedestal");
        SteamVR_TrackedObject rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)rightController.index);
    }

    void Update()
    {

        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            controllerInside = false;
            if (!isReplacement)
                EnlargeNetwork();
            else
                BringBackOriginal();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Controller")
        {
            controllerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.tag == "Controller")
        {
            controllerInside = false;
        }
    }

    private void EnlargeNetwork()
    {
        Enlarged = true;
        var rigidbody = gameObject.AddComponent<Rigidbody>();
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
        rigidbody.angularDrag = float.PositiveInfinity;
        var interactableObject = gameObject.AddComponent<VRTK_InteractableObject>();
        interactableObject.isGrabbable = true;
        var grabAttach = gameObject.AddComponent<VRTK_FixedJointGrabAttach>();
        grabAttach.precisionGrab = true;
        grabAttach.breakForce = float.PositiveInfinity;

        oldParent = transform.parent;
        oldLocalPosition = transform.localPosition;
        oldScale = transform.localScale;
        oldRotation = transform.rotation;

        transform.parent = null;
        transform.position = pedestal.transform.position + new Vector3(0, 1, 0);
        transform.localScale = new Vector3(.7f, .7f, .7f);
        transform.rotation = pedestal.transform.rotation;
        transform.Rotate(-90f, 0, 0);

        var replacement = Instantiate(replacementPrefab);
        replacement.transform.parent = oldParent;
        replacement.transform.localPosition = oldLocalPosition;
        replacement.transform.rotation = oldRotation;
        replacement.transform.localScale = oldScale;

        var replacementScript = replacement.GetComponent<NetworkCenter>();
        replacementScript.isReplacement = true;
        replacementScript.replacing = this;
    }

    private void BringBackOriginal()
    {
        if (isReplacement)
        {
            replacing.BringBackOriginal();
            Destroy(gameObject);
        }
        else
        {
            Enlarged = false;
            transform.parent = oldParent;
            transform.localPosition = oldLocalPosition;
            transform.localScale = oldScale;
            transform.rotation = oldRotation;
        }
    }
}
