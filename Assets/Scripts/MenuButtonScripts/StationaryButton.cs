using UnityEngine;

/// <summary>
/// Abstract class for all buttons that do not rotate when pressed.
/// </summary>
public abstract class StationaryButton : MonoBehaviour
{
    public SteamVR_TrackedObject rightController;
    public TextMesh descriptionText;
    public Sprite standardTexture;
    public Sprite highlightedTexture;
    // all buttons must override this variable's get property
    abstract protected string Description
    {
        get;
    }

    protected SteamVR_Controller.Device device;
    protected bool controllerInside;
    protected SpriteRenderer spriteRenderer;

   protected virtual void Start()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Controller"))
        {
            descriptionText.text = Description;
            spriteRenderer.sprite = highlightedTexture;
            controllerInside = true;
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Controller"))
        {
            descriptionText.text = "";
            spriteRenderer.sprite = standardTexture;
            controllerInside = false;
        }
    }

}

