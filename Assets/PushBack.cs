using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

public class PushBack : MonoBehaviour
{
    public SteamVR_TrackedObject rightController;
    public float distanceMultiplier = 0.5f;
    public float scaleMultiplier = 0.5f;

    private SteamVR_Controller.Device device;
    private Ray ray;
    private RaycastHit hit;
    private Transform raycastingSource;
    private bool push;
    private bool pull;
    private Vector3 orgPos;
    private Vector3 orgScale;
    private Quaternion orgRot;
    // Use this for initialization
    void Start()
    {
        rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
        device = SteamVR_Controller.Input((int)rightController.index);
        orgPos = transform.position;
        orgRot = transform.rotation;
        orgScale = transform.localScale;
    }

    // Update is called once per frame
    void Update()
    {
        if (push)
        {
            raycastingSource = rightController.transform;
            ray = new Ray(raycastingSource.position, raycastingSource.forward);
            if (Physics.Raycast(ray, out hit) && push)
            {
                Debug.Log("PUSH BACK");
                Vector3 dir = hit.transform.position - device.transform.pos;
                if (hit.transform.GetComponent<NetworkCenter>())
                {
                    transform.LookAt(Vector3.zero);
                }
                else if (hit.transform.GetComponent<Heatmap>())
                {
                    transform.LookAt(Vector3.zero);
                    transform.Rotate(90f, 0, 0);
                }
                dir = dir.normalized;
                transform.position += dir * distanceMultiplier;
                transform.localScale += orgScale * scaleMultiplier;
            }
        }
        if (pull)
        {
            raycastingSource = rightController.transform;
            ray = new Ray(raycastingSource.position, raycastingSource.forward);
            if (Physics.Raycast(ray, out hit) && pull)
            {
                Debug.Log("PULL BACK");
                Vector3 dir = hit.transform.position - device.transform.pos;
                dir = -dir.normalized;
                transform.position += dir * distanceMultiplier;
                transform.localScale -= orgScale * scaleMultiplier;
            }
        }
        if (rightController == null)
        {
            //Debug.Log("Find right controller");
            rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();

        }
        if (device == null)
        {
            device = SteamVR_Controller.Input((int)rightController.index);
        }

        if (/*(GetComponent<GraphInteract>() != null && GetComponent<GraphInteract> ().enabled) || */ GetComponent<VRTK_InteractableObject>() != null && GetComponent<VRTK_InteractableObject>().enabled)
        {

            //Debug.Log("scale: " + transform.localScale + " pos: " + transform.position);
            //Debug.Log("pos: " + orgPos + " scale: " + orgScale);
            if (device.GetPressDown(SteamVR_Controller.ButtonMask.Touchpad))
            {
                Vector2 touchpad = (device.GetAxis(Valve.VR.EVRButtonId.k_EButton_Axis0));
                if (touchpad.y > 0.7f)
                {
                    push = true;

                }
                if (touchpad.y < -0.7f)
                {
                    //               Debug.Log("GET BACK TO ORIGINAL");
                    //               transform.position = orgPos;
                    //transform.localScale = orgScale;
                    //               transform.rotation = orgRot;
                    pull = true;
                }
            }
        }
        if (device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
        {
            push = false;
            pull = false;
        }
    }

}
