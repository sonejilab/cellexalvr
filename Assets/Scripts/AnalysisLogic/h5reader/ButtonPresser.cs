using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CellexalVR.General;

public class ButtonPresser : MonoBehaviour
{
    // Start is called before the first frame update
    public BoxCollider collider;
    public ReferenceManager referenceManager;
    private SteamVR_TrackedObject rightController;
    private SteamVR_Controller.Device device;
    private bool controllerInside;
    [SerializeField]private Color color;

    void Start()
    {
        if (!referenceManager)
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }
        rightController = referenceManager.rightController;
        collider.size = new Vector3(70,30,1);
        collider.center = new Vector3(0,-15,0);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
        {
            controllerInside = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
        {
            controllerInside = false;
        }
    }

    private void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            Transform sphere = rightController.transform.Find("h5sphere");
            h5sphereScript script; 
            if (!sphere)
            {
                sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere).transform;
                script = sphere.gameObject.AddComponent<h5sphereScript>();
                sphere.parent = rightController.transform;
                sphere.localPosition = new Vector3(0, 0, 0.12f);
                sphere.localScale = Vector3.one * 0.03f;
                sphere.name = "h5sphere";
            }
            else
            {
                script = sphere.gameObject.GetComponent<h5sphereScript>();
            }
            sphere.GetComponent<Renderer>().material.color = color;
            script.color = color;
            SphereCollider collider = sphere.GetComponent<SphereCollider>();
            collider.isTrigger = true;
        }
    }
}
