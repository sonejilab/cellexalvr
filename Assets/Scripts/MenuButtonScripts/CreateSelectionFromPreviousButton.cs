using System;
using UnityEngine;

class CreateSelectionFromPreviousButton : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public TextMesh description;

    private SteamVR_TrackedObject rightController;
    private CellManager cellManager;
    private SteamVR_Controller.Device device;
    private new Renderer renderer;
    private bool controllerInside = false;
    private Color color;
    private string indexName;

    void Awake()
    {
        renderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        rightController = referenceManager.rightController;
        cellManager = referenceManager.cellManager;
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            cellManager.ColorByIndex(indexName);
        }
    }

    public void SetIndex(string indexName)
    {
        //color = network.GetComponent<Renderer>().material.color;
        //GetComponent<Renderer>().material.color = color;
        color = GetComponent<Renderer>().material.color;
        this.indexName = indexName;
        description.text = indexName;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            renderer.material.color = Color.white;
            controllerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            renderer.material.color = color;
            controllerInside = false;
        }
    }
}
