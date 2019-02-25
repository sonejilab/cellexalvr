///<summary>
/// Represents a button used for creating a heatmap from a cell selection.
///</summary>
public class CreateHeatmapButton : CellexalButton
{
    //public GeneDistance gd;
    private HeatmapGenerator heatmapGenerator;
    private GameManager gameManager;

    public string statsMethod;

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
        //CellexalEvents.CreatingHeatmap.AddListener(TurnOff);z
    }

    public override void Click()
    {
        heatmapGenerator.CreateHeatmap();
        //gd.CreateManyPlots();
    }

    private void TurnOn()
    {
        SetButtonActivated(true);
    }

    private void TurnOff()
    {
        SetButtonActivated(false);
        spriteRenderer.sprite = deactivatedTexture;
    }
}
