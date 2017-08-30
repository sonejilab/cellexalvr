using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represent the magnifying tool. The tool is a sphere that moves all graphpoints it collides with away from its center.
/// </summary>
class MagnifierTool : MonoBehaviour
{
    private bool recalc = false;
    private Dictionary<Transform, Vector3> pointsToMagnify = new Dictionary<Transform, Vector3>();

    private void Update()
    {
        //if (recalc) return;
        foreach (KeyValuePair<Transform, Vector3> pair in pointsToMagnify)
        {
            var graphPointTransform = pair.Key;
            var originPos = pair.Value;
            var dir = (originPos - transform.position).normalized;
            var dist = Vector3.Distance(transform.position, originPos);
            // if the graphpoint is sufficiently close to the center, we only offset it linearly based on it's distance from the center
            // if it is further away, we should offset it less and less
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
        // Check which graphpoints are now in the graph, since it might have moved.
        foreach (Collider c in Physics.OverlapSphere(transform.position, 0.1337f))
        {
            if (c.gameObject.CompareTag("Graph"))
                pointsToMagnify[c.transform] = c.transform.position;
        }
    }

    private void OnDisable()
    {
        // this script is disabled while the user is grabbing and holding the graph.
        foreach (KeyValuePair<Transform, Vector3> pair in pointsToMagnify)
        {
            pair.Key.position = pair.Value;
        }
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
