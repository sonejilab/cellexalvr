using UnityEngine;

/// <summary>
/// Abstract general purpose class that represents a button on the menu.
/// </summary>
public abstract class CellexalButton : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public TextMesh descriptionText;

    private int frameCount;
    private string laserColliderName = "[RightController]BasePointerRenderer_ObjectInteractor_Collider";
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
        this.tag = "Menu Controller Collider";
    }

    protected virtual void Update()
    {
        frameCount++;
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            Click();
        }
        // Button sometimes stays active even though ontriggerexit should have been called.
        // To deactivate button again check every 10th frame if laser pointer collider is colliding.
        if (frameCount % 10 == 0)
        {
            bool inside = false;
            Collider[] collidesWith = Physics.OverlapBox(transform.position, new Vector3(5.5f, 5.5f, 5.5f)/2, Quaternion.identity);
            foreach(Collider col in collidesWith)
            {
                if (col.gameObject.name == laserColliderName)
                {
                    inside = true;
                    return;
                }
            }
            if (descriptionText.text == Description)
            {
                descriptionText.text = "";
            }

            controllerInside = inside;
            SetHighlighted(inside);
            frameCount = 0;
        }
    }

    protected abstract void Click();

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
        if (activate)
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
        if (other.gameObject.name == laserColliderName)
        {
            descriptionText.text = Description;
            controllerInside = true;
            SetHighlighted(true);
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (!buttonActivated) return;
        if (other.gameObject.name == laserColliderName)
        {
            if (descriptionText.text == Description)
            {
                descriptionText.text = "";
            }
            controllerInside = false;
            SetHighlighted(false);
        }
    }

    public virtual void SetHighlighted(bool highlight)
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
        if(!highlight)
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
