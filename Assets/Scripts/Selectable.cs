using UnityEngine;

public class Selectable : MonoBehaviour
{

    public SelectionToolHandler selectionToolHandler;
    public CombinedGraph.CombinedGraphPoint graphPoint;

    private void OnTriggerEnter(Collider other)
    {
        selectionToolHandler.AddGraphpointToSelection(graphPoint);
    }
}
