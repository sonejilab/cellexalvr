using UnityEngine;
using VRTK;
/// <summary>
/// Represents the button for turning on and off the laser pointers.
/// </summary>
public class LasersButton : CellexalButton
{

    private ControllerModelSwitcher controllerModelSwitcher;

    protected override string Description
    {
        get { return "Toggle Lasers"; }
    }

    protected override void Awake()
    {
        base.Awake();
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
    }

    protected override void Click()
    {
        // turn both off only if both are on, otherwise turn both on.
        //laser1.enabled = !laser1.enabled;
        bool enabled = controllerModelSwitcher.DesiredModel == ControllerModelSwitcher.Model.TwoLasers;
        if (enabled)
        {
            controllerModelSwitcher.TurnOffActiveTool(true);
        }
        else
        {
            controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.TwoLasers;
            controllerModelSwitcher.ActivateDesiredTool();
        }
    }

}
