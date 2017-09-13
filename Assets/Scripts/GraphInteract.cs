using VRTK;

/// <summary>
/// This class handles what happens when a graph is interacted with.
/// </summary>
class GraphInteract : VRTK_InteractableObject
{
    public MagnifierTool magnifier;
    private bool magnifierDisabled;

    public override void OnInteractableObjectGrabbed(InteractableObjectEventArgs e)
    {
        // turn off the magnifying tool script so it won't distort the graphs.
        magnifierDisabled = magnifier.gameObject.activeSelf;
        magnifier.enabled = false;
        base.OnInteractableObjectGrabbed(e);
    }

    public override void OnInteractableObjectUngrabbed(InteractableObjectEventArgs e)
    {
        magnifier.enabled = true;
        base.OnInteractableObjectUngrabbed(e);
    }
}
