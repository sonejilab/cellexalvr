using UnityEngine;

/// <summary>
/// Represents on GraphPoint, the spheres that make up the graphs.
/// </summary>
public class GraphPoint : MonoBehaviour
{

    private Cell cell;
    private float x, y, z;
    private Material defaultMat;
    private Color selectedColor;
    private Graph defaultParent;
    private Renderer graphPointRenderer;
    public string GraphName { get { return defaultParent.GraphName; } }

    public void Start()
    {
        graphPointRenderer = GetComponent<Renderer>();
    }

    public void SetCoordinates(Cell cell, float x, float y, float z, Vector3 graphAreaSize)
    {
        this.cell = cell;
        this.x = x;
        this.y = y;
        this.z = z;
        defaultMat = Resources.Load("SphereDefault", typeof(Material)) as Material;
        cell.AddGraphPoint(this);
    }

    public string GetLabel()
    {
        return cell.Label;
    }

    public void SetMaterial(Material material)
    {
        graphPointRenderer.material = material;
    }

    public Material GetMaterial()
    {
        return graphPointRenderer.material;
    }

    public void ResetCoords()
    {
        transform.localPosition = new Vector3(x, y, z);
        //hard-coded to current sphere size
        transform.localScale = new Vector3(2.5f, 2.5f, 2.5f);
        transform.SetParent(defaultParent.transform);
        Rigidbody rig = GetComponent<Rigidbody>();
        if (rig != null)
        {
            Destroy(rig);
        }
        defaultMat = Resources.Load("SphereDefault", typeof(Material)) as Material;
        SetMaterial(defaultMat);
    }

    public Cell GetCell()
    {
        return cell;
    }

    public void SaveParent(Graph parent)
    {
        defaultParent = parent;
    }

    public void ResetColor()
    {
        graphPointRenderer.material = Resources.Load("SphereDefault", typeof(Material)) as Material;
    }
}
