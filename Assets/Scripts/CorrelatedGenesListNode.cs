using UnityEngine;

/// <summary>
/// This class represents a node in the list of correlated genes.
/// </summary>
public class CorrelatedGenesListNode : MonoBehaviour
{
    public TextMesh textMesh;
    private string label;
    public string GeneName
    {
        get { return label; }
        set { label = value; textMesh.text = value; }
    }
    private new Renderer renderer;

    private void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    /// <summary>
    /// Sets the list node's material to a new material.
    /// </summary>
    /// <param name="newMaterial"> The new material. </param>
    public void SetMaterial(Material newMaterial)
    {
        renderer.material = newMaterial;
    }
}