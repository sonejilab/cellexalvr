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
        base.StartTouching(currentTouchingObject);
        node.Highlight();
    }

    public override void StopTouching(GameObject previousTouchingObject)
    {
        base.StopTouching(previousTouchingObject);
        node.UnHighlight();
    }

    public override void StartUsing(GameObject currentUsingObject)
    {
        base.StartUsing(currentUsingObject);
        //print("using " + node.Label);
        cellManager.ColorGraphsByGene(node.Label.ToLower());
    }
}
