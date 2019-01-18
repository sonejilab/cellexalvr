/// <summary>
/// Represents the button that toggles the magnifier tool.
/// </summary>
class MagnifierToolButton : CellexalToolButton
{

    protected override ControllerModelSwitcher.Model ControllerModel
    {
        get { return ControllerModelSwitcher.Model.Magnifier; }
    }

    protected override string Description
    {
        get { return "Toggle magnifier tool"; }
    }

    private void Start()
    {
        SetButtonActivated(false);
    }
}

