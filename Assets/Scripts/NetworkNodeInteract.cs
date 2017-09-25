using UnityEngine;
using VRTK;

/// <summary>
/// This class holds the logic for interacting with nodes in an enlarged network.
/// </summary>
public class NetworkNodeInteract : VRTK_InteractableObject
{
    public NetworkNode node;
    private ReferenceManager referenceManager;
    private CellManager cellManager;

    protected override void Awake()
    {
        base.Awake();
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        cellManager = referenceManager.cellManager;
    }

    /// <summary>
    /// Called when a node is touched. Highlights the node and all outgoing edges.
    /// </summary>
    public override void StartTouching(GameObject currentTouchingObject)
    {
        node.Highlight();
        base.StartTouching(currentTouchingObject);
    }

    /// <summary>
    /// Called when a node is no longer touched. Un-highlights the node and all outgoing edges.
    /// </summary>
    public override void StopTouching(GameObject previousTouchingObject)
    {
        node.UnHighlight();
        base.StopTouching(previousTouchingObject);
    }

    /// <summary>
    /// Called when the node is used. A node is used by pointing at it with a laser pointer and pressing the trigger button.
    /// Colors all graphs by the gene that this node represents.
    /// </summary>
    public override void StartUsing(GameObject currentUsingObject)
    {
        base.StartUsing(currentUsingObject);
        //print("using " + node.Label);
        cellManager.ColorGraphsByGene(node.Label.ToLower());
        referenceManager.gameManager.InformColorGraphsByGene(node.Label.ToLower());
    }
}
