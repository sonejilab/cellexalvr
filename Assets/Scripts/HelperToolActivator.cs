using System;
using UnityEngine;

/// <summary>
/// This class represents the cylinder that appears with the loader and helps users who don't know how anything works.
/// </summary>
public class HelperToolActivator : MonoBehaviour
{
    public ReferenceManager referenceManager;
    public TextMesh descriptionText;

    private ControllerModelSwitcher controllerModelSwitcher;
    private int numControllersInside = 0;
    private string deactivatedDescription = "Put both controllers here\nif you need some help";
    private string activatedDescription = "Put both controllers here\nif you no longer need help";

    private void Start()
    {
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Controller"))
        {
            numControllersInside++;
            // The help tool activates when both controllers are inside 
            if (numControllersInside == 2)
            {
                bool helpToolActivated = controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.HelpTool;
                if (!helpToolActivated)
                {
                    controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.HelpTool;
                    controllerModelSwitcher.HelpToolShouldStayActivated = true;
                    controllerModelSwitcher.ActivateDesiredTool();
                }
                else
                {
                    controllerModelSwitcher.HelpToolShouldStayActivated = false;
                    controllerModelSwitcher.TurnOffActiveTool(false);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Controller"))
        {
            numControllersInside--;
        }
    }

    /// <summary>
    /// Switches the text on the sign on the helptoolactivator cylinder.
    /// </summary>
    /// <param name="activate"> True if the help tool is being activated, false if it is being deactivated. </param>
    public void SwitchText(bool activate)
    {
        if (activate)
        {
            descriptionText.text = activatedDescription;
        }
        else
        {
            descriptionText.text = deactivatedDescription;
        }
    }
}
