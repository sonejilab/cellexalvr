using System.Collections.Generic;
using UnityEngine;
using VRTK;

/// <summary>
/// This class represents a tool attached to the controller that moves graphpoints apart when too close to the tool
/// </summary>
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

            if (dist < .01808f)
            {
                graphPointTransform.position = originPos + dist * 5 * dir;
            }
            else
            {
                var l = (0.013f / (dist * .9f + 0.06f) - 0.08f);
                if (l < 0)
                    l = 0;
                graphPointTransform.position = originPos + l * dir;
            }
            //graphPointTransform.GetComponent<SphereCollider>().center = originPos;
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
