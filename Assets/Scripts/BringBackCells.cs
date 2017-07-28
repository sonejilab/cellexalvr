using UnityEngine;

/// <summary>
/// This class brings back cells that fall down under the floor.
/// </summary>
public class BringBackCells : MonoBehaviour
{

    private void OnTriggerEnter(Collider other)
    {
        var parent = other.transform.parent;
        if (parent == null) return;

        var cellsToLoad = parent.GetComponent<CellsToLoad>();
        if (cellsToLoad == null) return;

        parent.GetComponent<Rigidbody>().velocity = Vector3.zero;
        cellsToLoad.ResetPosition();
    }
}
