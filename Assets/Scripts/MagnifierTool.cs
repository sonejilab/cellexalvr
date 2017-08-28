using System.Collections.Generic;
using UnityEngine;
using VRTK;

class MagnifierTool : MonoBehaviour
{

    private Dictionary<Transform, Vector3> pointsToMagnify = new Dictionary<Transform, Vector3>();


    private void Update()
    {
        foreach (KeyValuePair<Transform, Vector3> pair in pointsToMagnify)
        {
            var graphPointTransform = pair.Key;
            var originPos = pair.Value;
            var dir = (originPos - transform.position).normalized;
            var dist = Vector3.Distance(transform.position, originPos);
            // if the graphpoint is sufficiently close to the center, we only offset it linearly based on it's distance from the center
            // if it is further away, we should offset it less and less
            float l;
            if (dist < 0.02f)
            {
                l = dist * 5;
            }
            else
            {
                l = dist * -1.2f + 0.12f;
                // if the graphoint is far away from the center we might get negative values, which we don't want
                if (l < 0)
                    l = 0;
            }
            graphPointTransform.position = originPos + l * dir;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Graph"))
        {
            //other.GetComponentInParent<Graph>().GetComponent<VRTK_InteractableObject>().isGrabbable = false;
            pointsToMagnify[other.transform] = other.transform.position;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Graph"))
        {
            //if (pointsToMagnify.Count == 0)
            //other.GetComponentInParent<Graph>().GetComponent<VRTK_InteractableObject>().isGrabbable = true;
            other.transform.position = pointsToMagnify[other.transform];
            pointsToMagnify.Remove(other.transform);
        }
    }
}
