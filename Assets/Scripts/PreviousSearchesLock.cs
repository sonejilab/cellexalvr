using UnityEngine;

public class PreviousSearchesLock : MonoBehaviour
{

    public PreviousSearchesListNode searchListNode;
    public Texture unlockedNormalMaterial;
    public Texture lockedNormalMaterial;
    public Texture unlockedHighlightedMaterial;
    public Texture lockedHighlightedMaterial;
    private new Renderer renderer;

    void Start()
    {
        renderer = GetComponent<Renderer>();
    }

    public void ToggleSearchNodeLock()
    {
        searchListNode.Locked = !searchListNode.Locked;
        // update the texture
        if (searchListNode.Locked)
            renderer.material.mainTexture = lockedHighlightedMaterial;
        else
            renderer.material.mainTexture = unlockedHighlightedMaterial;
    }

    public void SetMaterial(Texture texture)
    {
        renderer.material.mainTexture = texture;
    }

    public void SetHighlighted(bool highlighted)
    {
        if (highlighted)
        {
            if (searchListNode.Locked)
                renderer.material.mainTexture = lockedHighlightedMaterial;
            else
                renderer.material.mainTexture = unlockedHighlightedMaterial;
        }
        else
        {
            if (searchListNode.Locked)
                renderer.material.mainTexture = lockedNormalMaterial;
            else
                renderer.material.mainTexture = unlockedNormalMaterial;
        }
    }
}
