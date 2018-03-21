using UnityEngine;

//TODO CELLEXAL: Make all buttons same scale/size.

/// <summary>
/// Abstract class for all buttons that do not rotate when pressed.
/// </summary>
public abstract class StationaryButton : CellexalButton
{
    public Sprite standardTexture;
    public Sprite highlightedTexture;
    public Sprite deactivatedTexture;

    public SpriteRenderer spriteRenderer;

    protected override void Awake()
    {
        base.Awake();
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!buttonActivated) return;
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            descriptionText.text = Description;
            spriteRenderer.sprite = highlightedTexture;
            controllerInside = true;
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (!buttonActivated) return;
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            // sometimes the controller moves to another button before exiting this one.
            // that other button will then (probably) change the description.
            // so we only change it back to nothing if that has not happened.
            if (descriptionText.text.Equals(Description))
            {
                descriptionText.text = "";
            }
            spriteRenderer.sprite = standardTexture;
            controllerInside = false;
        }
    }

    /// <summary>
    /// Turns this button on or off. Buttons that are off are not clickable, but their colliders are still active.
    /// </summary>
    /// <param name="activate"> True for turning this button on, false for turning it off. </param>
    public override void SetButtonActivated(bool activate)
    {
        if (activate)
        {
            spriteRenderer.sprite = standardTexture;
        }
        else
        {
            spriteRenderer.sprite = deactivatedTexture;
            controllerInside = false;
        }
        buttonActivated = activate;
    }

    /// <summary>
    /// Tells this button that the menu it is attached to has turned off.
    /// </summary>
    public void MenuTurnedOff()
    {
        if (spriteRenderer)
        {
            if (buttonActivated)
            {
                spriteRenderer.sprite = standardTexture;
            }
            else
            {
                spriteRenderer.sprite = deactivatedTexture;
            }
        }
        controllerInside = false;
    }
}

