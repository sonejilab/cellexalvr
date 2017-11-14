using UnityEngine;
using VRTK;

/// <summary>
/// This class handles what happens when a graph is interacted with.
/// </summary>
class NetworkHandlerInteract : VRTK_InteractableObject
{
    public MagnifierTool magnifier;

    public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
    {

        // moving many triggers really pushes what unity is capable of
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }
        base.OnInteractableObjectGrabbed(e);
    }

    public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
    {
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = true;
        }
        base.OnInteractableObjectUngrabbed(e);
    }
}