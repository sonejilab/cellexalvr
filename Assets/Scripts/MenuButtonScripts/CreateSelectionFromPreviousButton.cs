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
    private int[] selectionGroups;

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
            referenceManager.cellManager.CreateNewSelection(graphName, selectionCellNames, selectionGroups);
        }
    }

    public void SetSelection(string graphName, string selectionName, string[] selectionCellNames, int[] selectionColors)
    {
        description.text = selectionName;
        this.graphName = graphName;
        this.selectionCellNames = selectionCellNames;
        this.selectionGroups = selectionColors;
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
