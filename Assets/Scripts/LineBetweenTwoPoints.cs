using UnityEngine;
/// <summary>
/// Represents a line between two graphpoints and moves the line accordingly when the graphpoints move.
/// </summary>
class LineBetweenTwoPoints : MonoBehaviour
{

    public Transform t1, t2;
    public GraphPoint graphPoint;
    public SelectionToolHandler selectionToolHandler;
    public Selectable cube;

    private LineRenderer lineRenderer;
    private Vector3 middle;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetPositions(new Vector3[] { t1.position, t2.position });
        middle = (t1.position + t2.position) / 2f;
        cube.transform.position = middle;
        cube.selectionToolHandler = selectionToolHandler;
        cube.graphPoint = graphPoint;
    }

    private void Update()
    {
        if (t1.hasChanged || t2.hasChanged)
        {
            lineRenderer.SetPosition(0, t1.position);
            lineRenderer.SetPosition(1, t2.position);
            middle = (t1.position + t2.position) / 2f;
            cube.transform.position = middle;
        }
    }
}
