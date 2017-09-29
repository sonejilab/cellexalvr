using UnityEngine;

/// <summary>
/// This class represents the cylinder that appears with the loader and helps users who don't know how anything works.
/// </summary>
class HelperToolActivator : MonoBehaviour
{

    public ReferenceManager referenceManager;

    private HelperTool helpTool;
    private ControllerModelSwitcher controllerModelSwitcher;

    private void Start()
    {
        helpTool = referenceManager.helpTool;
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Controller"))
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

