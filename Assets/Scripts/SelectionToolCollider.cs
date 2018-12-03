using UnityEngine;

/// <summary>
/// This class sole purpose is to forward collision events to the selection tool handler
/// </summary>
public class SelectionToolCollider : MonoBehaviour
{

    public SelectionToolHandler selectionToolHandler;

    void OnTriggerEnter(Collider other)
    {

        var cubeOnLine = other.gameObject.GetComponent<Selectable>();
        if (cubeOnLine != null)
        {
            selectionToolHandler.AddGraphpointToSelection(cubeOnLine.graphPoint);
            int group = selectionToolHandler.currentColorIndex;
            foreach (Selectable sel in cubeOnLine.graphPoint.lineBetweenCellsCubes)
            {
                sel.GetComponent<Renderer>().material.color = selectionToolHandler.Colors[group];
            }
            selectionToolHandler.referenceManager.gameManager.InformCubeColoured(cubeOnLine.graphPoint.parent.name,
                                                                                    cubeOnLine.graphPoint.Label, group, selectionToolHandler.Colors[group]);
        }
    }

}
