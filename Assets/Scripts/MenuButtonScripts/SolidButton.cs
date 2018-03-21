using UnityEngine;

/// <summary>
/// This abstract base class represents a "solid" button. Solid buttons have meshrenderers and not spriterenderers like <see cref="StationaryButton"/>
/// </summary>
public abstract class SolidButton : CellexalButton
{
    protected new Renderer renderer;
    protected Color color;

    protected override void Awake()
    {
        base.Awake();
        renderer = GetComponent<Renderer>();
        color = renderer.material.color;
    }

    protected virtual void Start()
    {
        rightController = referenceManager.rightController;
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            descriptionText.text = Description;
            renderer.material.color = Color.white;
            controllerInside = true;
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            if (descriptionText.text == Description)
            {
                descriptionText.text = "";
            }
            renderer.material.color = color;
            controllerInside = false;
        }
    }
}
