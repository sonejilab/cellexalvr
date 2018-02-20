using UnityEngine;

/// <summary>
/// Holds the directory name that the cells in the boxes should represent.
/// </summary>
public class CellsToLoad : MonoBehaviour
{

    private string directory;
    private bool graphsLoaded = false;
    private Vector3 defaultPosition;
    private Quaternion defaultRotation;

    public string Directory
    {
        get
        {
            graphsLoaded = true;
            return directory;
        }
        set
        {
            directory = value;
        }
    }

    public bool GraphsLoaded()
    {
        return graphsLoaded;
    }

    internal void ResetPosition()
    {
        transform.localPosition = defaultPosition;
        transform.localRotation = defaultRotation;
    }

    internal void SavePosition()
    {
        defaultPosition = transform.localPosition;
        defaultRotation = transform.localRotation;
    }
}
