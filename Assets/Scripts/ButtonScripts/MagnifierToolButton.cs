/// <summary>
/// Represents the button that toggles the magnifier tool.
/// </summary>
class MagnifierToolButton : CellexalToolButton
{

    protected override ControllerModelSwitcher.Model ControllerModel
    {
        get { return ControllerModelSwitcher.Model.Normal; }
    }

    protected override string Description
    {
        get { return ""; }
    }

    private void Start()
    {
        SetButtonActivated(false);
    }
}

