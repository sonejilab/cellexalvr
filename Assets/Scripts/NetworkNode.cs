using UnityEngine;
using System.Collections.Generic;
using TMPro;

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

    private ReferenceManager referenceManager;
    private List<NetworkNode> neighbours = new List<NetworkNode>();
    private List<LineRenderer> connections = new List<LineRenderer>();
    private List<Color> connectionColors = new List<Color>();
    private Vector3 normalScale;
    private Vector3 largerScale;
    private bool controllerInside;
    private SteamVR_TrackedObject rightController;
    private CellManager cellManager;
    private SteamVR_Controller.Device device;
    private bool edgesAdded;
    private float lineWidth = CellExAlConfig.NetworkLineSmallWidth;

    void Start()
    {
        GetComponent<Renderer>().sharedMaterial = standardMaterial;
        GetComponent<Collider>().enabled = false;
        normalScale = transform.localScale;
        largerScale = normalScale * 2f;
    }

    void Update()
    {
        // some math make the text not be mirrored
        transform.LookAt(2 * transform.position - CameraToLookAt.position);
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            cellManager.ColorGraphsByGene(Label.ToLower());
            referenceManager.gameManager.InformColorGraphsByGene(Label.ToLower());
        }
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
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            controllerInside = true;
            Highlight();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            controllerInside = false;
            UnHighlight();
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
    public void AddNeighbour(NetworkNode buddy)
    {
        // add this connection both ways
        neighbours.Add(buddy);
        buddy.neighbours.Add(this);
    }

    /// <summary>
    /// Makes this node and all outgoing edges big and white.
    /// </summary>
    public void Highlight()
    {
        GetComponent<Renderer>().sharedMaterial = highlightMaterial;
        transform.localScale = largerScale;
        foreach (LineRenderer r in connections)
        {
            r.material.color = Color.white;
            r.startWidth = r.endWidth = CellExAlConfig.NetworkLineSmallWidth * 3;
        }
    }

    /// <summary>
    /// Makes this node and all outgoing edges small and whatever color they were before.
    /// </summary>
    public void UnHighlight()
    {
        GetComponent<Renderer>().sharedMaterial = standardMaterial;
        transform.localScale = normalScale;
        for (int i = 0; i < connections.Count; ++i)
        {
            connections[i].material.color = connectionColors[i];
            connections[i].startWidth = connections[i].endWidth = CellExAlConfig.NetworkLineSmallWidth;
        }
    }

    /// <summary>
    /// Adds one edge for each buddy if there is not one already.
    /// </summary>
    public void AddEdges()
    {
        if (!edgesAdded)
        {
            NetworkGenerator networkGenerator = referenceManager.networkGenerator;
            Material[] LineMaterials = networkGenerator.LineMaterials;
            edgesAdded = true;
            foreach (NetworkNode buddy in neighbours)
            {
                if (!buddy.edgesAdded)
                {
                    GameObject edge = Instantiate(edgePrefab);
                    LineRenderer renderer = edge.GetComponent<LineRenderer>();
                    edge.transform.parent = transform.parent;
                    edge.transform.localPosition = Vector3.zero;
                    edge.transform.rotation = Quaternion.identity;
                    edge.transform.localScale = Vector3.one;
                    renderer.SetPositions(new Vector3[] { transform.localPosition, buddy.transform.localPosition });
                    // The colors are just random, they mean nothing. But they look pretty.
                    renderer.sharedMaterial = networkGenerator.LineMaterials[Random.Range(0, LineMaterials.Length)];
                    renderer.startWidth = renderer.endWidth = CellExAlConfig.NetworkLineSmallWidth;
                    connections.Add(renderer);
                    connectionColors.Add(renderer.material.color);
                    buddy.connections.Add(renderer);
                    buddy.connectionColors.Add(renderer.material.color);
                }
            }
        }
    }

}
