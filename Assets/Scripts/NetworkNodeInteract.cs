using UnityEngine;
using VRTK;

public class NetworkNodeInteract : VRTK_InteractableObject
{
    public NetworkNode node;
    public CellManager cellManager;

    protected override void Awake()
    {
        base.Awake();
        cellManager = GameObject.Find("CellManager").GetComponent<CellManager>();
    }

    public override void StartTouching(GameObject currentTouchingObject)
    {
        node.Highlight();
        base.StartTouching(currentTouchingObject);
    }

    public override void StopTouching(GameObject previousTouchingObject)
    {
        node.UnHighlight();
        base.StopTouching(previousTouchingObject);
    }

    public override void StartUsing(GameObject currentUsingObject)
    {
        base.StartUsing(currentUsingObject);
        //print("using " + node.Label);
        cellManager.ColorGraphsByGene(node.Label.ToLower());
    }
}
