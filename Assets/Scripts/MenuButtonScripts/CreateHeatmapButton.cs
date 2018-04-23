///<summary>
/// Represents a button used for creating a heatmap from a cell selection.
///</summary>
public class CreateHeatmapButton : CellexalButton
{

    private HeatmapGenerator heatmapGenerator;
    private GameManager gameManager;


    protected override string Description
    {
        get { return "Create heatmap"; }
    }

    protected void Start()
    {
        heatmapGenerator = referenceManager.heatmapGenerator;
        gameManager = referenceManager.gameManager;
        SetButtonActivated(false);
        CellexalEvents.SelectionConfirmed.AddListener(TurnOn);
        CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    protected override void Click()
    {
        SetButtonActivated(false);
        heatmapGenerator.CreateHeatmap();
        gameManager.InformCreateHeatmap();
        CellexalEvents.HeatmapCreated.Invoke();
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
