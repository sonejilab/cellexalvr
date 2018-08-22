
///<summary>
/// Represents a button used for toggling the selection tool.
///</summary>
public class SelectionToolButton : CellexalButton
{

    private MenuRotator rotator;
    private ControllerModelSwitcher controllerModelSwitcher;
    private SelectionToolHandler selectionToolHandler;
    private bool menuActive = false;
    //private bool buttonsInitialized = false;

    protected override string Description
    {
        get { return "Toggle selection tool"; }
    }
    private void Start()
    {
        rotator = referenceManager.menuRotator;
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
        selectionToolHandler = referenceManager.selectionToolHandler;
        SetButtonActivated(false);
        CellexalEvents.GraphsLoaded.AddListener(TurnOn);
        CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    protected override void Click()
    {

        if (controllerModelSwitcher.DesiredModel != ControllerModelSwitcher.Model.SelectionTool)
        {
            controllerModelSwitcher.DesiredModel = ControllerModelSwitcher.Model.SelectionTool;
            selectionToolHandler.gameObject.SetActive(true);
            controllerModelSwitcher.ActivateDesiredTool();
        }
        else
        {
            controllerModelSwitcher.TurnOffActiveTool(true);
            selectionToolHandler.gameObject.SetActive(false);
        }
        if (menuActive && rotator.SideFacingPlayer == MenuRotator.Rotation.Front)
        {
            rotator.RotateLeft();
        }
    }

    private void TurnOn()
    {
        SetButtonActivated(true);
    }

    private void TurnOff()
    {
        SetButtonActivated(false);
    }
}
