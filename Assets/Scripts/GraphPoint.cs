using UnityEngine;

/// <summary>
/// This class represents a graphpoint, the spheres that make up the graphs.
/// </summary>
public class GraphPoint : MonoBehaviour
{
    public Graph Graph;

    private float x, y, z;
    private MeshRenderer graphPointRenderer;
	public string label;

    #region Properties

    public Cell Cell { get; private set; }

    public int CurrentGroup { get; set; }

    public string GraphName
    {
        get { return Graph.GraphName; }
    }


    public string Label
    {
        get { return Cell.Label; }
    }

    /// <summary>
    /// The material used when rendering this graphpoint. 
    /// Unity can do some pretty heavy batching when graphpoints share materials, therefore, 
    /// use only the materials defined in the graphmanager.
    /// </summary>
    public Material Material
    {
        get { return graphPointRenderer.sharedMaterial; }
        set { graphPointRenderer.sharedMaterial = value; }
    }

    #endregion

    public void Start()
    {
        graphPointRenderer = GetComponent<MeshRenderer>();
        CurrentGroup = -1;
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
        this.Cell = cell;
		label = cell.Label;
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
        CurrentGroup = -1;
        SetOutLined(false, -1);
        // Color = Color.white;
    }

    /// <summary>
    /// Sets the outline of a graphpoint
    /// </summary>
    /// <param name="outline"> True if the graphpoint should be outlined, false if it should not. </param>
    /// <param name="group"> The group that this graphpoint belongs to. It will be colored based on that group in a selection. -1 indicates that a graphpoint does not belong to a group. </param>
    public void SetOutLined(bool outline, int group)
    {
        if (outline)
        {
            graphPointRenderer.sharedMaterial = Graph.graphManager.SelectedMaterialsOutline[group];
        }
        else
        {
            if (group == -1)
            {
                graphPointRenderer.sharedMaterial = Graph.graphManager.defaultGraphPointMaterial;
            }
            else
            {
                graphPointRenderer.sharedMaterial = Graph.graphManager.SelectedMaterials[group];
            }
        }
    }
}
