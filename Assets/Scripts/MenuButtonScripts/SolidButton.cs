using UnityEngine;

/// <summary>
/// This abstract base class represents a "solid" button. Solid buttons have meshrenderers and not spriterenderers like <see cref="StationaryButton"/>
/// </summary>
public abstract class SolidButton : MonoBehaviour
{
    public ReferenceManager referenceManager;

    protected SteamVR_TrackedObject rightController;
    protected SteamVR_Controller.Device device;
    protected new Renderer renderer;
    protected bool controllerInside = false;
    protected Color color;

    protected virtual void Awake()
    {
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
            renderer.material.color = Color.white;
            controllerInside = true;
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            renderer.material.color = color;
            controllerInside = false;
        }
    }
}
