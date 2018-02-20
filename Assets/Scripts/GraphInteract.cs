using UnityEngine;
using VRTK;

/// <summary>
/// Handles what happens when a graph is interacted with.
/// </summary>
class GraphInteract : VRTK_InteractableObject
{
    public MagnifierTool magnifier;

    public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
    {
        // turn off the magnifying tool script so it won't distort the graphs.
        magnifier.enabled = false;
        // moving many triggers really pushes what unity is capable of
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = false;
        }
        base.OnInteractableObjectGrabbed(e);
    }

    public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
    {
        magnifier.enabled = true;
        foreach (Collider c in GetComponentsInChildren<Collider>())
        {
            c.enabled = true;
        }
        base.OnInteractableObjectUngrabbed(e);
    }
}
