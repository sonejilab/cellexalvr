using UnityEngine;

public class PreviousSearchesList : MonoBehaviour
{

    public CellManager cellManager;
    public PreviousSearchesListNode topListNode;
    public SteamVR_TrackedController rightController;
    public SteamVR_Controller.Device device;
    public Material normalMaterial;
    public Material selectedMaterial;
    private Transform raycastingSource;
    private Ray ray;
    private RaycastHit hit;
    private LayerMask layer;
    private PreviousSearchesListNode listNode;
    private PreviousSearchesListNode lastHitListNode;
    private PreviousSearchesLock searchLock;
    private PreviousSearchesLock lastHitLock;
    private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;

    void Start()
    {
        raycastingSource = rightController.transform;
        device = SteamVR_Controller.Input((int)rightController.controllerIndex);
    }

    void Update()
    {
        ray = new Ray(raycastingSource.position, raycastingSource.forward);
        if (Physics.Raycast(ray, out hit))
        {
            listNode = hit.transform.gameObject.GetComponent<PreviousSearchesListNode>();
            searchLock = hit.transform.gameObject.GetComponent<PreviousSearchesLock>();
            if (listNode != null)
            {
                // handle the list node
                if (listNode != lastHitListNode)
                {
                    if (lastHitListNode != null)
                        lastHitListNode.SetMaterial(normalMaterial);
                    lastHitListNode = listNode;
                    listNode.SetMaterial(selectedMaterial);
                }
                if (device.GetPressDown(triggerButton))
                {
                    if (listNode.GeneName != "")
                    {
                        cellManager.ColorGraphsByPreviousExpression(listNode.Index);
                    }
                }
            }
            else if (searchLock != null)
            {
                // handle the lock
                if (searchLock != lastHitLock)
                {
                    if (lastHitLock != null)
                        lastHitLock.SetHighlighted(false);
                    lastHitLock = searchLock;
                    searchLock.SetHighlighted(true);
                }
                if (device.GetPressDown(triggerButton))
                {
                    searchLock.ToggleSearchNodeLock();

                }
            }
        }
        else
        {
            listNode = null;
            searchLock = null;
        }
        // when the raycaster leaves an object we must un-highlight it
        if (listNode == null && lastHitListNode != null)
        {
            lastHitListNode.SetMaterial(normalMaterial);
            lastHitListNode = null;
        }
        else if (searchLock == null && lastHitLock != null)
        {
            lastHitLock.SetHighlighted(false);
            lastHitLock = null;
        }
    }

    public void UpdateList(string newGeneName)
    {
        topListNode.UpdateList(newGeneName);
    }

    public void ClearList()
    {
        foreach (PreviousSearchesListNode node in GetComponentsInChildren<PreviousSearchesListNode>())
        {
            node.Locked = false;
            node.GeneName = "";
        }
        foreach (PreviousSearchesLock lockButton in GetComponentsInChildren<PreviousSearchesLock>())
        {
            lockButton.SetHighlighted(false);
        }
    }
}
