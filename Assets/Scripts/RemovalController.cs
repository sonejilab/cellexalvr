using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemovalController : MonoBehaviour {

    public Material inactiveMat;
    public Material activeMat;
    public ReferenceManager referenceManager;

    private bool controllerInside;
    private bool delete;
    private float fade = 0;
    private Transform target;
    private float speed;
    private float targetScale;
    private float shrinkSpeed;
    private GameObject objectToDelete;

    private SteamVR_TrackedObject rightController;
    private SteamVR_Controller.Device device;
    //Use this for initialization

    void Start()
    {
        rightController = referenceManager.rightController;
        speed = 1.5f;
        shrinkSpeed = 2f;
        targetScale = 0.1f;
    }

    // Update is called once per frame
    void Update()
    {
        if (device == null)
        {
            rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
            device = SteamVR_Controller.Input((int)rightController.index);
        }
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            delete = true;
        }
        if (delete)
        {
            DeleteObject(objectToDelete);
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.tag == "HeatBoard" || other.gameObject.GetComponent<NetworkHandler>())
        {
            controllerInside = true;
            objectToDelete = other.gameObject;
            GetComponent<Light>().color = Color.red;
            GetComponent<Light>().range = 0.05f;
            GetComponent<MeshRenderer>().material = activeMat;
            transform.localScale = Vector3.one * 0.04f;
        }
        if (other.CompareTag("HeatBoard"))
        {
            CellexalEvents.HeatmapBurned.Invoke();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        GetComponent<MeshRenderer>().material = inactiveMat;
        GetComponent<Light>().color = Color.white;
        transform.localScale = Vector3.one * 0.03f;
        GetComponent<Light>().range = 0.04f;
        controllerInside = false;
    }

    void DeleteObject(GameObject obj)
    {
        if (!obj)
        {
            delete = false;
            GetComponent<MeshRenderer>().material = inactiveMat;
            return;
        }
        float step = speed * Time.deltaTime;
        obj.transform.position = Vector3.MoveTowards(obj.transform.position, transform.position, step);
        obj.transform.localScale -= Vector3.one * Time.deltaTime * shrinkSpeed;
        obj.transform.Rotate(Vector3.one * Time.deltaTime * 100);
        if (obj.transform.localScale.x <= targetScale)
        {
            CellexalLog.Log("DELETED OBJECT " + obj.name);
            delete = false;
            Destroy(obj);
            GetComponent<MeshRenderer>().material = inactiveMat;
            GetComponent<Light>().color = Color.white;
            transform.localScale = Vector3.one * 0.03f;
            GetComponent<Light>().range = 0.04f;
        }
    }
}
