using UnityEngine;

/// <summary>
/// Represents on GraphPoint, the spheres that make up the graphs.
/// </summary>
public class GraphPoint : MonoBehaviour
{
    public Graph Graph;
    public Shader normalShader;
    public Shader outlineShader;

    private Cell cell;
    private float x, y, z;
    private MeshRenderer graphPointRenderer;
    private Color defaultColor = new Color(1, 1, 1);

    #region Properties

    public string GraphName
    {
        get { return Graph.GraphName; }
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
        graphPointRenderer = GetComponent<MeshRenderer>();
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
        transform.SetParent(Graph.transform);
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
        Graph = parent;
    }

    /// <summary>
    /// Resets the color of this graphpoint.
    /// </summary>
    public void ResetColor()
    {
        graphPointRenderer.material = Resources.Load("SphereDefault", typeof(Material)) as Material;
    }

    /// <summary>
    /// Sets the outline of the graphpoint.
    /// </summary>
    /// <param name="col"> The color that should be used when outlining, any color with 0 alpha to remove the outline. <param>
    public void Outline(Color col)
    {
        if (col.a !=  0)
        {
            graphPointRenderer.material.shader = outlineShader;
            // Set the outline color to a lighter version of the new color
            float outlineR = col.r + (1 - col.r) / 2;
            float outlineG = col.g + (1 - col.g) / 2;
            float outlineB = col.b + (1 - col.b) / 2;
            graphPointRenderer.material.SetColor("_OutlineColor", new Color(outlineR, outlineG, outlineB));
        }
        else
        {
            graphPointRenderer.material.shader = normalShader;
        }
    }
}
