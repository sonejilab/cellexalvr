using UnityEngine;

/// <summary>
/// Represents on GraphPoint, the spheres that make up the graphs.
/// </summary>
public class GraphPoint : MonoBehaviour
{

    private Cell cell;
    private float x, y, z;
    private bool selected = false;
    private Material defaultMat;
    private Color selectedColor;
    private Graph defaultParent;
    public string GraphName { get { return defaultParent.GraphName; } }

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

    public bool IsSelected()
    {
        return selected;
    }

    public void SetSelected(bool isSelected)
    {
        selected = isSelected;
    }

    public void SetMaterial(Material material)
    {
        GetComponent<Renderer>().material = material;
    }

    public Material GetMaterial()
    {
        return GetComponent<Renderer>().material;
    }

    public Material GetDefaultMaterial()
    {
        return defaultMat;
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
        selected = false;
        defaultMat = Resources.Load("SphereDefault", typeof(Material)) as Material;
        SetMaterial(defaultMat);
    }

    public Vector3 GetCoordinates()
    {
        return new Vector3(x, y, z);
    }

    public Cell GetCell()
    {
        return cell;
    }

    public void SaveParent(Graph parent)
    {
        defaultParent = parent;
    }

}
