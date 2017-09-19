using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class represents the tab buttons on top of the toggle arcs menu.
/// </summary>
public class ArcMenuTab : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public GameObject tab;
    private SteamVR_TrackedObject rightController;
    private ToggleArcsSubMenu arcsSubMenu;
    private bool controllerInside = false;
    private SteamVR_Controller.Device device;
    private List<GameObject> buttons = new List<GameObject>();
    private MeshRenderer meshRenderer;
    private Color standardColor = Color.gray;
    private Color highlightColor = Color.white;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        rightController = referenceManager.rightController;
        arcsSubMenu = referenceManager.arcsSubMenu;
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            arcsSubMenu.TurnOffTab();
            SetTabActive(true);
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
    /// Sets the buttons that this tab is responsible for.
    /// </summary>
    /// <param name="buttons"> A List with the buttons. </param>
    public void SetButtons(List<GameObject> buttons)
    {
        foreach (GameObject obj in buttons)
        {
            this.buttons.Add(obj);
        }
    }

    /// <summary>
    /// Changes the color of the button to either its highlighted color or standard color.
    /// </summary>
    /// <param name="highlight"> True if the button should be highlighted, false otherwise. </param>
    public void SetHighlighted(bool highlight)
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

    /// <summary>
    /// Show or hides all buttons that this tab contains.
    /// </summary>
    /// <param name="active"> True if this tab should be shown, false if hidden. </param>
    public void SetTabActive(bool active)
    {
        foreach (Transform sibling in tab.transform)
        {
            // We don't want to change the state of the tab buttons, they should always be turned on. 
            if (ReferenceEquals(sibling.gameObject.GetComponent<ArcMenuTab>(), null))
            {
                sibling.gameObject.SetActive(active);
            }
        }
    }
}

