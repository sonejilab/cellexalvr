using UnityEngine;
using VRTK;

/// <summary>
/// Handles what happens when a network center is interacted with.
/// </summary>
class NetworkCenterInteract : VRTK_InteractableObject
{
    public ReferenceManager referenceManager;

    public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
    {
        referenceManager.gameManager.InformDisableColliders(gameObject.name);
        if (grabbingObjects.Count == 1)
        {
            // moving many triggers really pushes what unity is capable of
            foreach (Collider c in GetComponentsInChildren<Collider>())
            {
                if (c.gameObject.name != "Ring" && c.gameObject.name != "Enlarged Network")
                {
                    c.enabled = false;
                }
                else if (c.gameObject.name == "Ring")
                {
                    ((MeshCollider)c).convex = true;
                }
            }
        }
        base.OnInteractableObjectGrabbed(e);
    }

    public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
    {
        referenceManager.gameManager.InformEnableColliders(gameObject.name);
        if (grabbingObjects.Count == 0)
        {
            foreach (Collider c in GetComponentsInChildren<Collider>())
            {
                if (c.gameObject.name != "Ring" && c.gameObject.name != "Enlarged Network")
                {
                    c.enabled = true;
                }
                else if (c.gameObject.name == "Ring")
                {
                    ((MeshCollider)c).convex = false;
                }

            }
        }
        base.OnInteractableObjectUngrabbed(e);
    }
}
