using UnityEngine;
/// <summary>
/// Represents a line between two graphpoints and moves the line accordingly when the graphpoints move.
/// </summary>
class LineBetweenTwoPoints : MonoBehaviour
{

    public Transform t1, t2;
    private LineRenderer lineRenderer;

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetPositions(new Vector3[] { t1.position, t2.position });
    }

    private void Update()
    {
        if (t1.hasChanged)
            lineRenderer.SetPosition(0, t1.position);

        if (t2.hasChanged)
            lineRenderer.SetPosition(1, t2.position);
    }
}
