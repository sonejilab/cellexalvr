using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RemoveErrorMessage : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public GameObject errorMessage;
    private SteamVR_TrackedObject rightController;
    private SteamVR_Controller.Device device;
    private bool controllerInside;

    private void Start()
    {
        if (!referenceManager)
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }
        if (!errorMessage)
        {
            errorMessage = GetComponentInParent<ErrorMessage>().gameObject;
        }
        rightController = referenceManager.rightController;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Menu Controller Collider"))
        {
            controllerInside = true;
            GetComponent<Renderer>().material.color = Color.red;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Menu Controller Collider"))
        {
            controllerInside = false;
            GetComponent<Renderer>().material.color = Color.white;
        }
    }

    private void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            Destroy(errorMessage);
        }
    }
}

