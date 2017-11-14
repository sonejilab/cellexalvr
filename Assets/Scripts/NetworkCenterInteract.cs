using UnityEngine;
using VRTK;

/// <summary>
/// This class handles what happens when a network is interacted with.
/// </summary>
class NetworkCenterInteract : VRTK_InteractableObject
{
    public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
    {
        // moving many triggers really pushes what unity is capable of
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            if (c.gameObject.name != "Ring")
                c.enabled = false;
            else
                ((MeshCollider)c).convex = true;
        }
        base.OnInteractableObjectGrabbed(e);
    }

    public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
    {
        if (grabbingObjects.Count == 0)
        {
            foreach (Collider c in GetComponentsInChildren<Collider>())
            {
                if (c.gameObject.name != "Ring")
                    c.enabled = true;
                else
                    ((MeshCollider)c).convex = false;
            }
        }
        base.OnInteractableObjectUngrabbed(e);
    }
}
