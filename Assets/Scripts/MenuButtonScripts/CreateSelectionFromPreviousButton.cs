using UnityEngine;

/// <summary>
/// Represents the buttons that are used to create new selections from old ones.
/// </summary>
class CreateSelectionFromPreviousButton : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public TextMesh description;

    private SteamVR_TrackedObject rightController;
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
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            referenceManager.cellManager.CreateNewSelection(graphName, selectionCellNames, selectionGroups);
        }
    }

    /// <summary>
    /// Set which selection this button represents.
    /// </summary>
    /// <param name="graphName"> Which graph the selection originated from. </param>
    /// <param name="selectionName"> The name of this selection. </param>
    /// <param name="selectionCellNames"> An array containing the cell names. </param>
    /// <param name="selectionGroups"> An array containing which groups the cells belonged to. </param>
    public void SetSelection(string graphName, string selectionName, string[] selectionCellNames, int[] selectionGroups)
    {
        description.text = selectionName;
        this.graphName = graphName;
        this.selectionCellNames = selectionCellNames;
        this.selectionGroups = selectionGroups;
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
