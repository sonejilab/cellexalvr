using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represent the magnifying tool. The tool is a sphere that moves all graphpoints it collides with away from its center.
/// </summary>
public class MagnifierTool : MonoBehaviour
{
    private Dictionary<Transform, Vector3> pointsToMagnify = new Dictionary<Transform, Vector3>();

    private void Update()
    {
        foreach (KeyValuePair<Transform, Vector3> pair in pointsToMagnify)
        {
            // The sphere is about 0.13 in radius.
            // Anything between 0.00 and 0.02 from the center will be linearly offset based on it's distance up to 0.10 distance away from the center.
            // Anything between 0.02 and 0.10 from the center will be put at the edge of the a sphere with a radius of 0.10.
            // Anything further away than 0.10 won't be moved.
            var graphPointTransform = pair.Key;
            var originPos = pair.Value;
            var dir = (originPos - transform.position).normalized;
            var dist = Vector3.Distance(transform.position, originPos);
            // l is the distance we want to move the graphpoint
            float l;
            if (dist < 0.02f)
            {
                l = dist * 5;
            }
            else
            {
                l = dist * -1.2f + 0.12f;
                // if the graphpoint is far away from the center we might get negative values, which we don't want
                if (l < 0)
                    l = 0;
            }
            graphPointTransform.position = originPos + l * dir;
        }
    }

    private void OnEnable()
    {
        //print("onenable");
        // Check which graphpoints are now in the graph, since it might have moved.
        // 0.1337 comes from multiplying the scale of this object (0.2675) with the radius of the sphere (0.5). All parent objects are of scale 1.
        foreach (Collider c in Physics.OverlapSphere(transform.position, 0.1337f))
        {
            if (c.gameObject.CompareTag("Graph"))
                pointsToMagnify[c.transform] = c.transform.position;
        }
    }

    private void OnDisable()
    {
        //print("ondisable");
        // this script is disabled while the user is grabbing and holding the graph.
        foreach (KeyValuePair<Transform, Vector3> pair in pointsToMagnify)
        {
            pair.Key.position = pair.Value;
        }
        // clear the list of graphpoints to move. Otherwise, if the graph moves and graphpoints are still inside the sphere, their origin positions won't have been updated.
        pointsToMagnify.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!enabled) return;
        if (other.gameObject.CompareTag("Graph"))
        {
            pointsToMagnify[other.transform] = other.transform.position;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!enabled) return;
        if (other.gameObject.CompareTag("Graph"))
        {
            if (pointsToMagnify.ContainsKey(other.transform))
                other.transform.position = pointsToMagnify[other.transform];
            pointsToMagnify.Remove(other.transform);
        }
    }
}
