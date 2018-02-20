using UnityEngine;
using VRTK;

public class PushBack : MonoBehaviour
{
    public SteamVR_TrackedObject rightController;
    public float distanceMultiplier = 0.5f;
    public float scaleMultiplier = 0.5f;
    public float maxScale;
    public float minScale;

    private SteamVR_Controller.Device device;
    private Ray ray;
    private RaycastHit hit;
    private Transform raycastingSource;
    private bool push;
    private bool pull;
    private Vector3 orgPos;
    private Vector3 orgScale;
    private Quaternion orgRot;

    void Start()
    {
        rightController = GameObject.Find("InputReader").GetComponent<ReferenceManager>().rightController;
        device = SteamVR_Controller.Input((int)rightController.index);
        orgPos = transform.position;
        orgRot = transform.rotation;
        orgScale = transform.localScale;
    }

    void Update()
    {
        if (rightController == null)
        {
            rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
        }

        device = SteamVR_Controller.Input((int)rightController.index);
        if (GetComponent<VRTK_InteractableObject>() != null && GetComponent<VRTK_InteractableObject>().enabled && !GetComponent<VRTK_InteractableObject>().IsGrabbed())
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
                    pull = true;
                }
            }
        }

        if (push)
        {
            raycastingSource = rightController.transform;
            ray = new Ray(raycastingSource.position, raycastingSource.forward);
            if (Physics.Raycast(ray, out hit) && push)
            {
                Vector3 newScale = transform.localScale + orgScale * scaleMultiplier;
                if (newScale.x > orgScale.x * maxScale)
                {
                    //print("not pulling back " + newScale.x + " " + orgScale.x);
                    return;
                }
                if (hit.transform.GetComponent<NetworkCenter>())
                {
                    transform.LookAt(Vector3.zero);
                }
                else if (hit.transform.GetComponent<Heatmap>())
                {
                    transform.LookAt(Vector3.zero);
                    transform.Rotate(90f, 0, 0);
                }
                Vector3 dir = hit.transform.position - device.transform.pos;
                dir = dir.normalized;
                transform.position += dir * distanceMultiplier;
                transform.localScale = newScale;
            }
        }
        if (pull)
        {
            raycastingSource = rightController.transform;
            ray = new Ray(raycastingSource.position, raycastingSource.forward);
            if (Physics.Raycast(ray, out hit) && pull)
            {
                Vector3 newScale = transform.localScale - orgScale * scaleMultiplier;
                // don't let the thing become smaller than what it was originally
                // this could cause some problems if the user rescales the objects while they are far away
                if (newScale.x < orgScale.x * minScale)
                {
                    //print("not pulling back " + newScale.x + " " + orgScale.x);
                    return;
                }
                Vector3 dir = hit.transform.position - device.transform.pos;
                dir = -dir.normalized;
                transform.position += dir * distanceMultiplier;
                transform.localScale = newScale;
            }
        }


        if (device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
        {
            push = false;
            pull = false;
        }
    }

}
