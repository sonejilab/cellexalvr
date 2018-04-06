using System;
using UnityEngine;
using UnityEngine.Events;

public class CalculatorButton : MonoBehaviour
{
    public UnityEvent action;
    public ReferenceManager referenceManager;

    private SteamVR_TrackedObject rightController;
    private SteamVR_Controller.Device device;
    private bool controllerInside;

    private void Start()
    {
        rightController = referenceManager.rightController;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            controllerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            controllerInside = false;
        }
    }

    private void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            action.Invoke();
        }
    }
}
