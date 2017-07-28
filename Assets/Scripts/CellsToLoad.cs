using System;
using UnityEngine;

public class CellsToLoad : MonoBehaviour
{

    private string directory;
    private bool graphsLoaded = false;
    private Vector3 defaultPosition;
    private Quaternion defaultRotation;

    public bool GraphsLoaded()
    {
        return graphsLoaded;
    }

    public string GetDirectory()
    {
        graphsLoaded = true;
        return directory;
    }

    public void SetDirectory(string name)
    {
        directory = name;
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
