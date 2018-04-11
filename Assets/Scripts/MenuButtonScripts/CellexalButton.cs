using UnityEngine;

/// <summary>
/// Abstract general purpose class that represents a button on the menu.
/// </summary>
public abstract class CellexalButton : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public TextMesh descriptionText;


    // all buttons must override this variable's get property
    /// <summary>
    /// A string that briefly explains what this button does.
    /// </summary>
    abstract protected string Description
    {
        get;
    }

    // These are drawn in the inspector through CellexalButtonEditor.cs
    [HideInInspector]
    public Color meshStandardColor = Color.black;
    [HideInInspector]
    public Color meshHighlightColor = Color.white;
    [HideInInspector]
    public Color meshDeactivatedColor = Color.grey;
    [HideInInspector]
    public Sprite standardTexture = null;
    [HideInInspector]
    public Sprite highlightedTexture = null;
    [HideInInspector]
    public Sprite deactivatedTexture = null;
    [HideInInspector]
    public int popupChoice = 0;

    protected SteamVR_TrackedObject rightController;
    protected SteamVR_Controller.Device device;
    protected SpriteRenderer spriteRenderer;
    protected MeshRenderer meshRenderer;
    [HideInInspector]
    public bool buttonActivated = true;
    public bool controllerInside = false;

    protected virtual void Awake()
    {
        if (referenceManager == null)
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }
        rightController = referenceManager.rightController;
        device = SteamVR_Controller.Input((int)rightController.index);
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        meshRenderer = gameObject.GetComponent<MeshRenderer>();
    }

    public virtual void SetButtonActivated(bool activate)
    {

        if (!activate)
        {
            descriptionText.text = "";
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = deactivatedTexture;
            }
            else if (meshRenderer != null)
            {
                meshRenderer.material.color = meshDeactivatedColor;
            }
        }
        else
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = standardTexture;
            }
            else if (meshRenderer != null)
            {
                meshRenderer.material.color = meshStandardColor;
            }
        }
        buttonActivated = activate;
        controllerInside = false;
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (!buttonActivated) return;
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            descriptionText.text = Description;
            controllerInside = true;
            SetHighlighted(true);
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (!buttonActivated) return;
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            if (descriptionText.text == Description)
            {
                descriptionText.text = "";
            }
            controllerInside = false;
            SetHighlighted(false);
        }
    }

    public void SetHighlighted(bool highlight)
    {
        if (highlight)
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = highlightedTexture;
            }
            else if (meshRenderer != null)
            {
                meshRenderer.material.color = meshHighlightColor;
            }
        }
        else
        {
            if (spriteRenderer != null)
            {
                spriteRenderer.sprite = standardTexture;
            }
            else if (meshRenderer != null)
            {
                meshRenderer.material.color = meshStandardColor;
            }
        }
    }
}
