using UnityEngine;

/// <summary>
/// This class brings back cells that are tossed away too far.
/// </summary>
public class BringBackCellsFloor : MonoBehaviour
{
    public GameObject throwMarker;
    private CellsToLoad reset;

    private void FixedUpdate()
    {
        if (reset != null)
        {
            var marker = Instantiate(throwMarker);
            marker.transform.position = reset.gameObject.transform.position;
            marker.transform.LookAt(Vector3.zero);
            marker.GetComponentInChildren<TextMesh>().text = Vector3.Distance(Vector3.zero, marker.transform.position) + " m!";
            reset.ResetPosition();
            reset = null;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        var parent = other.transform.parent;
        if (parent == null) return;

        var cellsToLoad = parent.GetComponent<CellsToLoad>();
        if (cellsToLoad == null) return;



        if (parent.GetComponent<Rigidbody>().velocity == Vector3.zero)
        {
            reset = cellsToLoad;
        }
    }
}
