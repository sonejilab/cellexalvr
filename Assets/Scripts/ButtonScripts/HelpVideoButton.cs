using UnityEngine;

/// <summary>
/// Represents the button that plays the relevant help video for a function.
/// </summary>
public class HelpVideoButton : CellexalButton
{
    private ControllerModelSwitcher controllerModelSwitcher;
    private HelperTool helpTool;

    protected override string Description
    {
        get { return ""; }
    }

    protected override void Click()
    {
        base.HelpClick();
    }

    protected override void Awake()
    {
        base.Awake();
    }

}
