using UnityEngine;

/// <summary>
/// Represents on GraphPoint, the spheres that make up the graphs.
/// </summary>
public class GraphPoint : MonoBehaviour
{

    private Cell cell;
    private float x, y, z;
    private Graph defaultParent;
    private Renderer graphPointRenderer;
    private Color defaultColor = new Color(1, 1, 1);

    #region Properties

    public string GraphName
    {
        get { return defaultParent.GraphName; }
    }

    public Cell Cell
    {
        get { return cell; }
    }

    public string Label
    {
        get { return cell.Label; }
    }

    public Color Color
    {
        get { return graphPointRenderer.material.color; }
        set { graphPointRenderer.material.color = value; }
    }

    #endregion

    public void Start()
    {
        graphPointRenderer = GetComponent<Renderer>();
    }

    /// <summary>
    /// Sets some variables.
    /// </summary>
    /// <param name="cell"> The cell that this graphpoint is representing. </param>
    /// <param name="x"> The x-coordinate. </param>
    /// <param name="y"> The y-coordinate. </param>
    /// <param name="z"> The z-coordinate. </param>
    public void SetCoordinates(Cell cell, float x, float y, float z)
    {
        this.cell = cell;
        this.x = x;
        this.y = y;
        this.z = z;
        cell.AddGraphPoint(this);
    }

    /// <summary>
    /// Resets the position, scale and color of this graphpoint.
    /// </summary>
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
        graphPointRenderer.material.color = defaultColor;
    }


    /// <summary>
    /// Saves this graphpoint's parent graph.
    /// </summary>
    /// <param name="parent"> The graph this graphpoint is part of. </param>
    public void SaveParent(Graph parent)
    {
        defaultParent = parent;
    }

    /// <summary>
    /// Resets the color of this graphpoint.
    /// </summary>
    public void ResetColor()
    {
        graphPointRenderer.material = Resources.Load("SphereDefault", typeof(Material)) as Material;
    }
}
