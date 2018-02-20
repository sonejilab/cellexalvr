using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents the list of the 10 previous searches of genes.
/// </summary>
public class PreviousSearchesList : MonoBehaviour
{
    public ReferenceManager referenceManager;

    public Material normalMaterial;
    public Material highlightedMaterial;
    public Texture searchLockNormalTexture;
    public Texture searchLockNormalHighlightedTexture;
    public Texture searchLockLockedTexture;
    public Texture searchLockLockedHighlightedTexture;
    public Texture correlatedGenesButtonTexture;
    public Texture correlatedGenesButtonHighlightedTexture;
    public Texture correlatedGenesButtonWorkingTexture;
    public List<PreviousSearchesLock> searchLocks = new List<PreviousSearchesLock>();
    public List<CorrelatedGenesButton> correlatedGenesButtons = new List<CorrelatedGenesButton>();

    private SteamVR_TrackedObject rightController;
    private SteamVR_Controller.Device device;
    private CellManager cellManager;
    private Transform raycastingSource;
    private Ray ray;
    private RaycastHit hit;
    private LayerMask layer;
    private PreviousSearchesListNode listNode;
    private PreviousSearchesListNode lastHitListNode;
    private PreviousSearchesLock searchLock;
    private PreviousSearchesLock lastHitLock;
    private CorrelatedGenesButton correlatedGenesButton;
    private CorrelatedGenesButton lastCorrelatedGenesButton;
    private CorrelatedGenesListNode correlatedGenesListNode;
    private CorrelatedGenesListNode lastCorrelatedGenesListNode;
    private ColoringOptionsButton coloringOptionsButton;
    private ColoringOptionsButton lastColoringOptionsButton;
    private Valve.VR.EVRButtonId triggerButton = Valve.VR.EVRButtonId.k_EButton_SteamVR_Trigger;
    private GameManager gameManager;

    private void Start()
    {
        rightController = referenceManager.rightController;
        cellManager = referenceManager.cellManager;
        gameManager = referenceManager.gameManager;
    }

    void Update()
    {
        raycastingSource = rightController.transform;
        device = SteamVR_Controller.Input((int)rightController.index);
        // this method is probably responsible for too much. oh well.
        ray = new Ray(raycastingSource.position, raycastingSource.forward);
        if (Physics.Raycast(ray, out hit))
        {
            // we may hit something that is not of any use to us.
            // so we check if we hit anything interesting.
            listNode = hit.transform.gameObject.GetComponent<PreviousSearchesListNode>();
            searchLock = hit.transform.gameObject.GetComponent<PreviousSearchesLock>();
            correlatedGenesButton = hit.transform.gameObject.GetComponent<CorrelatedGenesButton>();
            correlatedGenesListNode = hit.transform.gameObject.GetComponent<CorrelatedGenesListNode>();
            coloringOptionsButton = hit.transform.gameObject.GetComponent<ColoringOptionsButton>();
            // see if we hit anything
            if (listNode != null)
            {
                // handle the list node
                if (listNode != lastHitListNode)
                {
                    if (lastHitListNode != null)
                        lastHitListNode.SetMaterial(normalMaterial);
                    lastHitListNode = listNode;
                    listNode.SetMaterial(highlightedMaterial);
                }
                if (device.GetPressDown(triggerButton))
                {
                    if (listNode.GeneName != "")
                    {
                        cellManager.ColorGraphsByPreviousExpression(listNode.GeneName);
                        gameManager.InformColorGraphByPreviousExpression(listNode.GeneName);
                    }
                }
            }
            else if (searchLock != null)
            {
                // handle the lock
                if (searchLock != lastHitLock)
                {
                    if (lastHitLock != null)
                    {
                        if (lastHitLock.Locked)
                            lastHitLock.SetTexture(searchLockLockedTexture);
                        else
                            lastHitLock.SetTexture(searchLockNormalTexture);

                    }
                    lastHitLock = searchLock;
                    if (searchLock.Locked)
                        searchLock.SetTexture(searchLockLockedHighlightedTexture);
                    else
                        searchLock.SetTexture(searchLockNormalHighlightedTexture);

                }
                if (device.GetPressDown(triggerButton))
                {
                    searchLock.ToggleSearchNodeLock();
                    gameManager.InformSearchLockToggled(searchLocks.IndexOf(searchLock));

                    if (searchLock.Locked)
                        searchLock.SetTexture(searchLockLockedHighlightedTexture);
                    else
                        searchLock.SetTexture(searchLockNormalHighlightedTexture);
                }
            }
            else if (correlatedGenesButton != null)
            {
                // handle the calculate correlated genes button
                if (lastCorrelatedGenesButton != correlatedGenesButton)
                {
                    if (lastCorrelatedGenesButton != null)
                        lastCorrelatedGenesButton.SetTexture(correlatedGenesButtonTexture);
                    lastCorrelatedGenesButton = correlatedGenesButton;
                    correlatedGenesButton.SetTexture(correlatedGenesButtonHighlightedTexture);
                }

                if (device.GetPressDown(triggerButton))
                {
                    correlatedGenesButton.SetTexture(correlatedGenesButtonWorkingTexture);
                    string geneName = correlatedGenesButton.listNode.GeneName;
                    int index = correlatedGenesButtons.IndexOf(correlatedGenesButton);
                    // we need to split on a space here beacause the genename will actually be "<genename> <coloring mode>"
                    // so we have to get rid of the coloring mode, it's not interesting when calculating correlated genes
                    referenceManager.correlatedGenesList.CalculateCorrelatedGenes(index, geneName = geneName.Split(' ')[0]);
                    gameManager.InformCalculateCorrelatedGenes(index, geneName);
                }
            }
            else if (correlatedGenesListNode != null)
            {
                // handle the correlated gene button
                if (lastCorrelatedGenesListNode != correlatedGenesListNode)
                {
                    if (lastCorrelatedGenesListNode != null)
                        lastCorrelatedGenesListNode.SetMaterial(normalMaterial);
                    lastCorrelatedGenesListNode = correlatedGenesListNode;
                    correlatedGenesListNode.SetMaterial(highlightedMaterial);
                }

                if (device.GetPressDown(triggerButton))
                {
                    string gene = correlatedGenesListNode.GeneName;
                    cellManager.ColorGraphsByGene(gene);
                    gameManager.InformColorGraphsByGene(gene);
                }
            }
            else if (coloringOptionsButton != null)
            {
                // handle the gene expression coloring button button
                if (lastColoringOptionsButton != coloringOptionsButton)
                {
                    if (lastColoringOptionsButton != null)
                        lastColoringOptionsButton.SetMaterial(normalMaterial);
                    lastColoringOptionsButton = coloringOptionsButton;
                    coloringOptionsButton.SetMaterial(highlightedMaterial);
                }

                if (device.GetPressDown(triggerButton))
                {
                    coloringOptionsButton.PressButton();
                }
            }
        }
        else
        {
            // the raycast hit nothing
            listNode = null;
            searchLock = null;
            correlatedGenesButton = null;
            correlatedGenesListNode = null;
            coloringOptionsButton = null;
        }
        // when the raycaster leaves an object we must un-highlight it
        if (listNode == null && lastHitListNode != null)
        {
            lastHitListNode.SetMaterial(normalMaterial);
            lastHitListNode = null;
        }
        else if (searchLock == null && lastHitLock != null)
        {
            if (lastHitLock.Locked)
                lastHitLock.SetTexture(searchLockLockedTexture);
            else
                lastHitLock.SetTexture(searchLockNormalTexture);
            lastHitLock = null;
        }
        else if (correlatedGenesButton == null && lastCorrelatedGenesButton != null)
        {
            lastCorrelatedGenesButton.SetTexture(correlatedGenesButtonTexture);
            lastCorrelatedGenesButton = null;
        }
        else if (correlatedGenesListNode == null && lastCorrelatedGenesListNode != null)
        {
            lastCorrelatedGenesListNode.SetMaterial(normalMaterial);
            lastCorrelatedGenesListNode = null;
        }
        else if (coloringOptionsButton == null && lastColoringOptionsButton != null)
        {
            lastColoringOptionsButton.SetMaterial(normalMaterial);
            lastColoringOptionsButton = null;
        }
    }

    /// <summary>
    /// Clears the list.
    /// </summary>
    public void ClearList()
    {
        foreach (PreviousSearchesListNode node in GetComponentsInChildren<PreviousSearchesListNode>())
        {
            node.Locked = false;
            node.GeneName = "";
        }
        foreach (PreviousSearchesLock lockButton in GetComponentsInChildren<PreviousSearchesLock>())
        {
            lockButton.Locked = false;
            lockButton.SetTexture(searchLockNormalTexture);
        }
    }
}
