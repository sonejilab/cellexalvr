using SQLiter;
/// <summary>
/// Represents the button on the <see cref="ColorByGeneMenu"/> that queries the database for the most differentially expressed genes.
/// </summary>
public class QueryTopGenesButton : StationaryButton
{

    public SQLite.QueryTopGenesRankingMode mode;

    private CellManager cellmanager;

    protected override string Description
    {
        get
        {
            return "Calculate top genes";
        }
    }

    private void Start()
    {
        cellmanager = referenceManager.cellManager;
    }

    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            cellmanager.QueryTopGenes(mode);
        }
    }
}