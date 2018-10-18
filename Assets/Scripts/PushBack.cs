using UnityEngine;
using VRTK;

public class PushBack : MonoBehaviour
{
    public SteamVR_TrackedObject rightController;
    public float distanceMultiplier = 0.5f;
    public float scaleMultiplier = 0.5f;
    public float maxScale;
    public float minScale;
    public ReferenceManager referenceManager;

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
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        rightController = referenceManager.rightController;
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
        bool active = GetComponent<VRTK_InteractableObject>() != null && GetComponent<VRTK_InteractableObject>().enabled
            && !GetComponent<VRTK_InteractableObject>().IsGrabbed() && referenceManager.controllerModelSwitcher.ActualModel == ControllerModelSwitcher.Model.TwoLasers;
        if (active)
        {
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
            int layerMask = 1 << LayerMask.NameToLayer("GraphLayer");
            raycastingSource = rightController.transform;
            Physics.Raycast(raycastingSource.position, raycastingSource.TransformDirection(Vector3.forward), out hit, Mathf.Infinity, layerMask);
            //ray = new Ray(raycastingSource.position, raycastingSource.forward);
            if (hit.collider && push)
            {
                if (hit.transform.GetComponent<NetworkCenter>())
                {
                    transform.LookAt(Vector3.zero);
                }
                else if (hit.transform.GetComponent<Heatmap>())
                {
                    transform.LookAt(Vector3.zero);
                }
                Vector3 dir = hit.transform.position - raycastingSource.position;
                dir = dir.normalized;
                transform.position += dir * distanceMultiplier;
                //transform.localScale = newScale;
            }
        }
        if (pull)
        {
            raycastingSource = rightController.transform;
            int layerMask = 1 << LayerMask.NameToLayer("GraphLayer");
            raycastingSource = rightController.transform;
            Physics.Raycast(raycastingSource.position, raycastingSource.TransformDirection(Vector3.forward), out hit, 10, layerMask);
            if (hit.collider && pull)
            {
                // don't let the thing become smaller than what it was originally
                // this could cause some problems if the user rescales the objects while they are far away
                if (Vector3.Distance(hit.transform.position, raycastingSource.position) < 0.5f)
                {
                    //print("not pulling back " + newScale.x + " " + orgScale.x);
                    pull = false;
                    return;
                }
                Vector3 dir = hit.transform.position - raycastingSource.position;
                dir = -dir.normalized;
                transform.position += dir * distanceMultiplier;
            }
        }


        if (device.GetPressUp(SteamVR_Controller.ButtonMask.Touchpad))
        {
            push = false;
            pull = false;
        }
    }

}
