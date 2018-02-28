using UnityEngine;

/// <summary>
/// This class sole purpose is to forward collision events to the selection tool handler
/// </summary>
public class SelectionToolCollider : MonoBehaviour
{

    public SelectionToolHandler selectionToolHandler;

    void OnTriggerEnter(Collider other)
    {
        print("hit something");
        var graphpoint = other.gameObject.GetComponent<GraphPoint>();
        if (graphpoint != null)
        {
            selectionToolHandler.AddGraphpointToSelection(graphpoint);
            return;
        }
        print("hit cube");
        var cubeOnLine = other.gameObject.GetComponent<Selectable>();
        if (cubeOnLine != null)
        {
            selectionToolHandler.AddGraphpointToSelection(cubeOnLine.graphPoint);
            int group = selectionToolHandler.currentColorIndex;
            cubeOnLine.GetComponent<Renderer>().material.color = selectionToolHandler.Colors[group];
        }
    }
}
