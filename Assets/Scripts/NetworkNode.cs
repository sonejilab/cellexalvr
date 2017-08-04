using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

/// <summary>
/// This class represents one node in a network, it handles the coloring of the connections and part of the network creation process,
/// </summary>
public class NetworkNode : MonoBehaviour
{
    public TextMesh geneName;
    public GameObject edgePrefab;
    public GameObject arcDescriptionPrefab;
    public Transform CameraToLookAt { get; set; }
    public NetworkCenter Center { get; set; }
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
    private List<NetworkNode> neighbours = new List<NetworkNode>();
    private List<LineRenderer> connections = new List<LineRenderer>();
    private Transform textTransform;
    private Color nodeColor;
    private List<Color> connectionColors = new List<Color>();
    private Vector3 normalScale;
    private Vector3 largerScale;
    private bool edgesAdded = false;
    private bool repositionedByBuddy = false;
    private bool repositionedBuddies = false;

    private float lineWidth = .001f;



    void Start()
    {
        textTransform = geneName.transform;
        nodeColor = GetComponent<Renderer>().material.color;
        normalScale = transform.localScale;
        largerScale = normalScale * 2f;
    }

    void Update()
    {
        // some math make the text not be mirrored
        textTransform.LookAt(2 * transform.position - CameraToLookAt.position);

    }

    public void AddNeighbour(NetworkNode buddy)
    {
        // add this connection both ways
        neighbours.Add(buddy);
        buddy.neighbours.Add(this);
    }

    public void Highlight()
    {
        GetComponent<Renderer>().material.color = Color.white;
        transform.localScale = largerScale;
        foreach (LineRenderer r in connections)
        {
            r.material.color = Color.white;
            r.startWidth = .005f;
            r.endWidth = .005f;
        }
    }

    public void UnHighlight()
    {
        GetComponent<Renderer>().material.color = nodeColor;
        transform.localScale = normalScale;
        for (int i = 0; i < connections.Count; ++i)
        {
            connections[i].material.color = connectionColors[i];
            connections[i].startWidth = lineWidth;
            connections[i].endWidth = lineWidth;
        }
    }

    public void AddEdges()
    {
        if (!edgesAdded)
        {
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
                    renderer.material.color = UnityEngine.Random.ColorHSV(0, 1, .6f, 1, .6f, 1);
                    connections.Add(renderer);
                    connectionColors.Add(renderer.material.color);
                    buddy.connections.Add(renderer);
                    buddy.connectionColors.Add(renderer.material.color);
                }
            }
        }
    }


}
