using UnityEngine;

/// <summary>
/// This class sole purpose is to forward collision events to the selection tool handler
/// </summary>
public class SelectionToolCollider : MonoBehaviour
{

    public SelectionToolHandler selectionToolHandler;

    void OnTriggerEnter(Collider other)
    {
        var graphPoint = other.gameObject.GetComponent<GraphPoint>();
        if (graphPoint != null)
        {
            selectionToolHandler.AddGraphpointToSelection(graphPoint);
            return;
        }
        var cubeOnLine = other.gameObject.GetComponent<Selectable>();
        if (cubeOnLine != null)
        {
            selectionToolHandler.AddGraphpointToSelection(cubeOnLine.graphPoint);
            int group = selectionToolHandler.currentColorIndex;
            cubeOnLine.GetComponent<Renderer>().material.color = selectionToolHandler.Colors[group];
            selectionToolHandler.referenceManager.gameManager.InformCubeColoured(cubeOnLine.graphPoint.GraphName, cubeOnLine.graphPoint.label, group, selectionToolHandler.Colors[group]);
        }
    }
}
