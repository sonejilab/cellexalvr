using UnityEngine;
using VRTK;

public class NetworkNodeInteract : VRTK_InteractableObject
{
    public NetworkNode node;
    public CellManager cellManager;
    public Color highlightColor;
    private Renderer networkNodeRenderer;
    private Color normalColor;

    protected override void Awake()
    {
        base.Awake();
        cellManager = GameObject.Find("CellManager").GetComponent<CellManager>();
        networkNodeRenderer = gameObject.GetComponent<Renderer>();
        normalColor = networkNodeRenderer.material.color;
    }

    public override void StartTouching(GameObject currentTouchingObject)
    {
        base.StartTouching(currentTouchingObject);
        networkNodeRenderer.material.color = highlightColor;
    }

    public override void StopTouching(GameObject previousTouchingObject)
    {
        base.StopTouching(previousTouchingObject);
        networkNodeRenderer.material.color = normalColor;
    }

    public override void StartUsing(GameObject currentUsingObject)
    {
        base.StartUsing(currentUsingObject);
        //print("using " + node.Label);
        cellManager.ColorGraphsByGene(node.Label.ToLower());
    }
}
