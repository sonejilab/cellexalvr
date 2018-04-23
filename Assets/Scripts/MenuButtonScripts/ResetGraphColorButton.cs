
///<summary>
/// Represents a button used for resetting the color and position of the graphs.
///</summary>
public class ResetGraphColorButton : CellexalButton
{

    private GraphManager graphManager;

    protected override string Description
    {
        get
        {
            return "Reset the color of all graphs";
        }
    }

    private void Start()
    {
        graphManager = referenceManager.graphManager;
        SetButtonActivated(false);
        CellexalEvents.GraphsLoaded.AddListener(OnGraphsLoaded);
        CellexalEvents.GraphsUnloaded.AddListener(OnGraphsUnloaded);
    }

    protected override void Click()
    {
        graphManager.ResetGraphsColor();
    }

    private void OnGraphsLoaded()
    {
        SetButtonActivated(true);
    }

    private void OnGraphsUnloaded()
    {
        SetButtonActivated(false);
    }
}
