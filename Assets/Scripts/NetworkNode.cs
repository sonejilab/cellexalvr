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
    private bool repositionedBuddies = false;

    void Start()
    {
        textTransform = geneName.transform;
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
                    buddy.PositionBuddies(offset /*+ new Vector3(0, 0, .1f)*/, buddyRepositionInc);
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
                    // place this edge in the middle between us and our buddy
                    LineRenderer renderer = edge.GetComponent<LineRenderer>();

                    edge.transform.parent = transform.parent;
                    Vector3 middlePoint = (transform.localPosition + buddy.transform.localPosition) / 2f;
                    //middlePoint.y = UnityEngine.Random.Range(-.5f, .5f);
                    renderer.SetPositions(new Vector3[] { transform.localPosition, middlePoint, buddy.transform.localPosition });
                    edge.transform.localPosition = Vector3.zero;
                    edge.transform.localScale = Vector3.one;
                    renderer.material.color = UnityEngine.Random.ColorHSV(0, 1, .8f, 1, .8f, 1);
                    //edge.transform.localPosition = (transform.localPosition + buddy.transform.localPosition) / 2f;
                    //float distance = Vector3.Distance(transform.localPosition, buddy.transform.localPosition);
                    //edge.transform.localScale = new Vector3(edge.transform.localScale.x, edge.transform.localScale.y, edge.transform.localScale.z * distance);
                    //edge.transform.LookAt(transform.parent);
                    //edge.transform.Rotate(0, 0, 90);

                    //edge.transform.parent = transform;
                }
            }
        }
    }
}
