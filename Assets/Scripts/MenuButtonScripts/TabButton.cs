using UnityEngine;

/// <summary>
/// This class represents the tab buttons on top of a tab.
/// </summary>
public class TabButton : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public Tab tab;
    public MenuWithTabs Menu;

    protected SteamVR_TrackedObject rightController;
    protected bool controllerInside = false;
    protected SteamVR_Controller.Device device;
    private MeshRenderer meshRenderer;
    private Color standardColor = Color.gray;
    private Color highlightColor = Color.white;

    protected virtual void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        rightController = referenceManager.rightController;

    }

    protected virtual void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            Menu.TurnOffAllTabs();
            tab.SetTabActive(true);
        }
    }

    protected void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            controllerInside = true;
            SetHighlighted(true);
        }
    }

    protected void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            controllerInside = false;
            SetHighlighted(false);
        }
    }

    /// <summary>
    /// Changes the color of the button to either its highlighted color or standard color.
    /// </summary>
    /// <param name="highlight"> True if the button should be highlighted, false otherwise. </param>
    public virtual void SetHighlighted(bool highlight)
    {
        if (highlight)
        {
            meshRenderer.material.color = highlightColor;
        }
        else
        {
            meshRenderer.material.color = standardColor;
        }
    }
}

