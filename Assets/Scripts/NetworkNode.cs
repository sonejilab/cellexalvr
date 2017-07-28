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
    private List<Arc> arcList = new List<Arc>();
    private bool edgesAdded = false;
    private bool repositionedByBuddy = false;
    private bool repositionedBuddies = false;

    private float lineWidth = .001f;

    private struct Arc
    {
        public LineRenderer renderer;
        public Transform t1, t2, t3;

        public Arc(LineRenderer renderer, Transform t1, Transform t2, Transform t3)
        {
            this.renderer = renderer;
            this.t1 = t1;
            this.t2 = t2;
            this.t3 = t3;
        }
    }

    void Start()
    {
        textTransform = geneName.transform;
        nodeColor = GetComponent<Renderer>().material.color;
    }

    void Update()
    {
        // some math make the text not be mirrored
        textTransform.LookAt(2 * transform.position - CameraToLookAt.position);
        foreach (Arc a in arcList)
        {
            Vector3 midPoint1 = Vector3.Lerp(transform.position, a.t1.transform.position, .5f);
            Vector3 midPoint2 = Vector3.Lerp(a.t2.transform.position, a.t3.transform.position, .5f);
            a.renderer.SetPositions(new Vector3[] { midPoint1, midPoint2 });
        }
    }

    public void AddNeighbour(NetworkNode buddy)
    {
        // add this connection both ways
        neighbours.Add(buddy);
        buddy.neighbours.Add(this);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Smaller Controller Collider" && transform.parent.GetComponent<NetworkCenter>().Enlarged)
        {
            GetComponent<Renderer>().material.color = Color.white;
            foreach (LineRenderer r in connections)
            {
                r.material.color = Color.white;
                r.startWidth = .005f;
                r.endWidth = .005f;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Smaller Controller Collider" && transform.parent.GetComponent<NetworkCenter>().Enlarged)
        {
            GetComponent<Renderer>().material.color = nodeColor;
            for (int i = 0; i < connections.Count; ++i)
            {
                connections[i].material.color = connectionColors[i];
                connections[i].startWidth = lineWidth;
                connections[i].endWidth = lineWidth;
            }
        }
    }

    public void PositionBuddies(Vector3 offset, Vector3 buddyRepositionInc)
    {
        // only reposition if this node has not already been reposition by one of it's buddies
        //if (repositionedByBuddy)
        //{
        //    return;
        //}
        repositionedByBuddy = true;
        Vector3 buddyRepositionAmount = buddyRepositionInc;
        if (!repositionedBuddies)
        {
            repositionedBuddies = true;
            foreach (NetworkNode buddy in neighbours)
            {
                if (!buddy.repositionedByBuddy)
                {
                    buddy.transform.localPosition = transform.localPosition + offset + buddyRepositionAmount;
                    buddyRepositionAmount += buddyRepositionInc;
                    buddy.repositionedByBuddy = true;
                }
            }
            foreach (NetworkNode buddy in neighbours)
            {
                if (!buddy.repositionedBuddies)
                    buddy.PositionBuddies(offset, buddyRepositionInc);
            }
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

    internal void AddArc(NetworkNode neighbour, NetworkNode otherNode, NetworkNode otherNodeNeighbour)
    {
        GameObject edge = Instantiate(edgePrefab);
        LineRenderer renderer = edge.GetComponent<LineRenderer>();
        edge.transform.parent = transform.parent;
        edge.transform.localPosition = Vector3.zero;
        edge.transform.rotation = Quaternion.identity;
        edge.transform.localScale = Vector3.one;
        Vector3 midPoint1 = Vector3.Lerp(transform.position, neighbour.transform.position, .5f);
        Vector3 midPoint2 = Vector3.Lerp(otherNode.transform.position, otherNodeNeighbour.transform.position, .5f);
        renderer.useWorldSpace = true;
        renderer.SetPositions(new Vector3[] { midPoint1, midPoint2 });
        arcList.Add(new Arc(renderer, neighbour.transform, otherNode.transform, otherNodeNeighbour.transform));

        GameObject arcText = Instantiate(arcDescriptionPrefab);
        arcText.transform.parent = transform.parent.parent;
        arcText.transform.position = Vector3.Lerp(midPoint1, midPoint2, .5f);
        arcText.GetComponent<TextRotator>().SetTransforms(transform, neighbour.transform, otherNode.transform, otherNodeNeighbour.transform);
        arcText.GetComponent<TextMesh>().text = geneName.text + " <-> " + neighbour.geneName.text;

        var center = transform.parent.GetComponent<NetworkCenter>();
        center.AddArc(edge);
        center.AddArc(arcText);
        var otherCenter = otherNode.transform.parent.GetComponent<NetworkCenter>();
        otherCenter.AddArc(edge);
        otherCenter.AddArc(arcText);
    }
}
