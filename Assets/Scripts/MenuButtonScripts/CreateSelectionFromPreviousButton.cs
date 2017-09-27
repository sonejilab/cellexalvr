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
    private string graphName;
    private string[] selectionCellNames;
    private Color[] selectionColors;

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
            referenceManager.cellManager.CreateNewSelectionFromArray(graphName, selectionCellNames, selectionColors);
        }
    }

    public void SetSelection(string graphName, string selectionName, string[] selectionCellNames, Color[] selectionColors)
    {
        description.text = selectionName;
        this.graphName = graphName;
        this.selectionCellNames = selectionCellNames;
        this.selectionColors = selectionColors;
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
            renderer.material.color = Color.black;
            controllerInside = false;
        }
    }
}
