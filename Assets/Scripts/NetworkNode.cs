using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class NetworkNode : MonoBehaviour
{
    public TextMesh geneName;
    public GameObject edgePrefab;
    public Transform CameraToLookAt { get; set; }
    public string Label { set { geneName.text = value; } }
    private List<NetworkNode> neighbours = new List<NetworkNode>();
    private Transform textTransform;
    private bool edgesAdded = false;
    private bool repositionedByBuddy = false;

    void Start()
    {
        textTransform = geneName.transform;
    }

    void Update()
    {
        textTransform.LookAt(2 * transform.position - CameraToLookAt.position);
    }

    public void AddNeighbour(NetworkNode buddy)
    {
        // add this connection both ways
        neighbours.Add(buddy);
        buddy.neighbours.Add(this);

        //buddy.transform.position = transform.position + new Vector3(.05f, 0, 0);
    }

    public void PositionBuddies()
    {
        // only reposition if this node has not already been reposition by one of it's buddies
        if (repositionedByBuddy)
            return;

        repositionedByBuddy = true;
        Vector3 buddyRepositionAmount = transform.localPosition;
        Vector3 buddyRepositionIncAmount = buddyRepositionAmount;
        foreach (NetworkNode buddy in neighbours)
        {
            buddy.repositionedByBuddy = true;
            buddy.transform.localPosition = transform.localPosition + buddyRepositionAmount;
            buddyRepositionAmount += buddyRepositionIncAmount;
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
                    edge.transform.parent = transform.parent;
                    // place this edge in the middle between us and our buddy
                    edge.transform.localPosition = Vector3.Lerp(transform.localPosition, buddy.transform.localPosition, .5f);
                    float distance = Vector3.Distance(transform.localPosition, buddy.transform.localPosition);
                    edge.transform.localScale = new Vector3(edge.transform.localScale.x, edge.transform.localScale.y, edge.transform.localScale.z * distance);
                    edge.transform.LookAt(transform.parent);
                    edge.transform.Rotate(0, 0, 90);
                }
            }
        }
    }
}
