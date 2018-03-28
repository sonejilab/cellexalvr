using UnityEngine;

/// <summary>
/// Abstract class that all panels around the keyboard should inherit.
/// </summary>
public abstract class ClickablePanel : MonoBehaviour
{
    public ReferenceManager referenceManager;

    protected new Renderer renderer;
    protected Material keyNormalMaterial;
    protected Material keyHighlightMaterial;
    protected Material keyPressedMaterial;

    protected virtual void Start()
    {
        renderer = gameObject.GetComponent<Renderer>();
    }

    /// <summary>
    /// Sets this panel's materials.
    /// </summary>
    /// <param name="keyNormalMaterial">The normal material.</param>
    /// <param name="keyHighlightMaterial">The material that should be used when the laser pointer is pointed at the button.</param>
    /// <param name="keyPressedMaterial">The material that should be used when the panel is pressed.</param>
    public virtual void SetMaterials(Material keyNormalMaterial, Material keyHighlightMaterial, Material keyPressedMaterial)
    {
        this.keyNormalMaterial = keyNormalMaterial;
        this.keyHighlightMaterial = keyHighlightMaterial;
        this.keyPressedMaterial = keyPressedMaterial;
    }

    public abstract void Click();

    /// <summary>
    /// Sets this panel to highlighted or not highlighted.
    /// </summary>
    /// <param name="highlight">True for highlighted, false for not highlighted.</param>
    public virtual void SetHighlighted(bool highlight)
    {
        if (!renderer)
        {
            renderer = gameObject.GetComponent<Renderer>();
        }
        if (highlight)
        {
            renderer.sharedMaterial = keyHighlightMaterial;
        }
        else
        {
            renderer.sharedMaterial = keyNormalMaterial;
        }
    }
}

