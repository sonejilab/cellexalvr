using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using System.Linq;

/// <summary>
/// Represents one node in a network, it handles the coloring of the connections and part of the network creation process,
/// </summary>
public class NetworkNode : MonoBehaviour
{
    public TextMeshPro geneName;
    public GameObject edgePrefab;
    public GameObject arcDescriptionPrefab;
    public Transform CameraToLookAt { get; set; }
    public NetworkCenter Center { get; set; }
    public Material standardMaterial;
    public Material highlightMaterial;
    private string label;
    public string Label
    {
        get { return label; }
        set
        {
            label = value;
            geneName.text = value;
        }
    }
    public Vector3[] LayoutPositions { get; set; } = new Vector3[2];

    private ReferenceManager referenceManager;
    public HashSet<NetworkNode> neighbours = new HashSet<NetworkNode>();
    public List<Tuple<NetworkNode, NetworkNode, LineRenderer, float>> edges = new List<Tuple<NetworkNode, NetworkNode, LineRenderer, float>>();
    private List<Color> edgeColors = new List<Color>();
    private Vector3 normalScale;
    private Vector3 largerScale;
    private bool controllerInside;
    private SteamVR_TrackedObject rightController;
    private CellManager cellManager;
    private SteamVR_Controller.Device device;
    private bool edgesAdded;
    private float lineWidth = CellexalConfig.NetworkLineSmallWidth;

    void Start()
    {
        GetComponent<Renderer>().sharedMaterial = standardMaterial;
        GetComponent<Collider>().enabled = false;
        normalScale = transform.localScale;
        largerScale = normalScale * 2f;

        this.name = geneName.text;
    }

    void Update()
    {
        // some math make the text not be mirrored
        transform.LookAt(2 * transform.position - CameraToLookAt.position);
    }

    public override bool Equals(object other)
    {
        var node = other as NetworkNode;
        if (node == null)
            return false;
        return label == node.label;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    /// <summary>
    /// Tells this networknode that the networkcenter it belongs to is being brought back to the networkhandler.
    /// </summary>
    public void BringBack()
    {
        // the networkcenter turns off this networknode's collider, so the controller is not inside any longer.
        controllerInside = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        bool active = Center.Enlarged;
        bool touched = other.gameObject.CompareTag("Menu Controller Collider") || other.gameObject.name.Equals("[RightController]BasePointerRenderer_ObjectInteractor_Collider");
        if (active && touched)
        {
            var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == this.name);
            controllerInside = true;
            foreach (GameObject obj in objects)
            {
                NetworkNode nn = obj.GetComponent<NetworkNode>();
                nn.Highlight();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            cellManager.ColorGraphsByGene(Label.ToLower(), referenceManager.graphManager.GeneExpressionColoringMethod);
            referenceManager.gameManager.InformColorGraphsByGene(Label.ToLower());
            controllerInside = false;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Menu Controller Collider") || other.gameObject.name.Equals("[RightController]BasePointerRenderer_ObjectInteractor_Collider"))
        {
            var objects = Resources.FindObjectsOfTypeAll<GameObject>().Where(obj => obj.name == this.name);
            controllerInside = false;
            foreach (GameObject obj in objects)
            {
                NetworkNode nn = obj.GetComponent<NetworkNode>();
                nn.UnHighlight();
            }
           
        }

    }

    public void SetReferenceManager(ReferenceManager referenceManager)
    {
        this.referenceManager = referenceManager;
        cellManager = referenceManager.cellManager;
        rightController = referenceManager.rightController;
    }

    /// <summary>
    /// Adds a neighbour to this node. A neughbour should be a gene that is correlated to this node's gene.
    /// This will also add this node as the neighbour's neighbour, so it's basically a bidirectional edge between two vertices.
    /// A gene may have many neighbours.
    /// </summary>
    /// <param name="buddy"> The new neighbour </param>
    public void AddNeighbour(NetworkNode buddy, float pcor)
    {
        // add this connection both ways
        neighbours.Add(buddy);
        buddy.neighbours.Add(this);

        NetworkGenerator networkGenerator = referenceManager.networkGenerator;
        Material[] LineMaterials = networkGenerator.LineMaterials;
        GameObject edge = Instantiate(edgePrefab);
        LineRenderer renderer = edge.GetComponent<LineRenderer>();
        edge.transform.parent = transform.parent;
        edge.transform.localPosition = Vector3.zero;
        edge.transform.rotation = Quaternion.identity;
        edge.transform.localScale = Vector3.one;
        // renderer.sharedMaterial = networkGenerator.LineMaterials[UnityEngine.Random.Range(0, LineMaterials.Length)];
        renderer.startWidth = renderer.endWidth = CellexalConfig.NetworkLineSmallWidth;
        renderer.enabled = false;
        var newEdge = new Tuple<NetworkNode, NetworkNode, LineRenderer, float>(this, buddy, renderer, pcor);
        edges.Add(newEdge);
        //edgeColors.Add(renderer.material.color);
        buddy.edges.Add(newEdge);
        //buddy.edgeColors.Add(renderer.material.color);

    }

    /// <summary>
    /// Makes this node and all outgoing edges big and white.
    /// </summary>
    public void Highlight()
    {
        GetComponent<Renderer>().sharedMaterial = highlightMaterial;
        transform.localScale = largerScale;
        foreach (var tuple in edges)
        {
            var line = tuple.Item3;
            line.material.color = Color.white;
            line.startWidth = line.endWidth = CellexalConfig.NetworkLineSmallWidth * 3;
        }
    }

    /// <summary>
    /// Makes this node and all outgoing edges small and whatever color they were before.
    /// </summary>
    public void UnHighlight()
    {
        GetComponent<Renderer>().sharedMaterial = standardMaterial;
        transform.localScale = normalScale;
        for (int i = 0; i < edges.Count; ++i)
        {
            var line = edges[i].Item3;
            line.material.color = edgeColors[i];
            line.startWidth = line.endWidth = CellexalConfig.NetworkLineSmallWidth;
        }
    }

    public List<NetworkNode> AllConnectedNodes()
    {
        List<NetworkNode> result = new List<NetworkNode>();
        AllConnectedNodesRec(ref result);
        return result;
    }

    private void AllConnectedNodesRec(ref List<NetworkNode> result)
    {
        result.Add(this);
        foreach (var neighbour in neighbours)
        {
            if (!result.Contains(neighbour))
            {
                neighbour.AllConnectedNodesRec(ref result);
            }
        }
    }

    public void RepositionEdges()
    {
        for (int i = 0; i < edges.Count; ++i)
        {
            edges[i].Item3.SetPositions(new Vector3[] { edges[i].Item1.transform.localPosition, edges[i].Item2.transform.localPosition });
        }
    }

    public void ColorEdges()
    {
        float minNegPcor = Center.MinNegPcor;
        float maxNegPcor = Center.MaxNegPcor;
        float minPosPcor = Center.MinPosPcor;
        float maxPosPcor = Center.MaxPosPcor;

        var colors = referenceManager.networkGenerator.LineMaterials;
        if (CellexalConfig.NetworkLineColoringMethod == 0)
        {
            int numColors = CellexalConfig.NumberOfNetworkLineColors;
            foreach (var edge in edges)
            {
                edge.Item3.enabled = true;
                float pcor = edge.Item4;
                if (pcor < 0f)
                {
                    int colorIndex;
                    // these are some special cases that can make the index go out of bounds
                    if (pcor == minNegPcor)
                    {
                        colorIndex = 0;
                    }
                    else if (pcor == maxNegPcor)
                    {
                        colorIndex = (numColors / 2) - 1;
                        if (colorIndex < 0)
                        {
                            colorIndex = 0;
                        }
                    }
                    else
                    {
                        colorIndex = (int)((1 - ((pcor - maxNegPcor) / (minNegPcor - maxNegPcor))) * (numColors / 2));
                    }
                    edge.Item3.material.color = colors[colorIndex].color;
                    edgeColors.Add(colors[colorIndex].color);
                }
                else
                {
                    int colorIndex;
                    if (pcor == maxPosPcor)
                    {
                        colorIndex = colors.Length - 1;
                    }
                    else
                    {
                        colorIndex = (int)(((pcor - minPosPcor) / (maxPosPcor - minPosPcor)) * (numColors / 2)) + (numColors / 2);
                    }
                    edge.Item3.material.color = colors[colorIndex].color;
                    edgeColors.Add(colors[colorIndex].color);
                }
            }
        }
        else if (CellexalConfig.NetworkLineColoringMethod == 1)
        {
            foreach (var edge in edges)
            {
                edge.Item3.enabled = true;
                int colorIndex = UnityEngine.Random.Range(0, colors.Length);
                edge.Item3.material.color = colors[colorIndex].color;
                edgeColors.Add(colors[colorIndex].color);
            }
        }
    }
}
