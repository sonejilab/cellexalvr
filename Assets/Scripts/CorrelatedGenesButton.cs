using UnityEngine;

/// <summary>
/// This class represents the button that calculates the correlated genes.
/// </summary>
public class CorrelatedGenesButton : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public PreviousSearchesListNode listNode;

    private CorrelatedGenesList correlatedGenesList;
    private new Renderer renderer;

    private void Start()
    {
        renderer = GetComponent<Renderer>();
        correlatedGenesList = referenceManager.correlatedGenesList;
    }

    /// <summary>
    /// Sets the texture of this button.
    /// </summary>
    /// <param name="newTexture"> The new texture. </param>
    public void SetTexture(Texture newTexture)
    {
        renderer.material.mainTexture = newTexture;
    }
}
