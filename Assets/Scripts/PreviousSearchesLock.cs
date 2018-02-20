using UnityEngine;

/// <summary>
/// Represents the locks next to the previous searches.
/// Pressing one of the locks makes the gene name next to it not move when a new gene is searched for.
/// </summary>
public class PreviousSearchesLock : MonoBehaviour
{

    public PreviousSearchesListNode searchListNode;
    public bool Locked { get; set; }

    private new Renderer renderer;

    void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    /// <summary>
    /// Toggles the lock.
    /// </summary>
    public void ToggleSearchNodeLock()
    {
        bool newState = !searchListNode.Locked;
        searchListNode.Locked = newState;
        Locked = newState;
    }

    public void SetTexture(Texture newTexture)
    {
        if (renderer != null)
            renderer.material.mainTexture = newTexture;
    }
}
