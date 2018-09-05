using UnityEngine;
using VRTK;

/// <summary>
/// Handles what happens when a network handler is interacted with.
/// </summary>
class NetworkHandlerInteract : VRTK_InteractableObject
{
    public ReferenceManager referenceManager;

    public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
    {
        referenceManager.gameManager.InformDisableColliders(gameObject.name);
        // moving many triggers really pushes what unity is capable of
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            if (c.gameObject.name == "Ring")
            {
                ((MeshCollider)c).convex = true;
            }
        }
        GetComponent<NetworkHandler>().ToggleNetworkColliders(false);
        base.OnInteractableObjectGrabbed(e);
    }

    public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
    {
        referenceManager.gameManager.InformEnableColliders(gameObject.name);
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            if (c.gameObject.name == "Ring")
            {
                ((MeshCollider)c).convex = false;
            }
        }
        GetComponent<NetworkHandler>().ToggleNetworkColliders(true);
        base.OnInteractableObjectUngrabbed(e);
    }
}