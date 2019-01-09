/// <summary>
/// Looks for new reports in the user specific folder. If there is one that is not yet in the list it is added.
/// </summary>
public class RefreshListButton : CellexalButton
{
    private ReportListGenerator reportListGenerator;

    protected override string Description
    {
        get
        {
            return "Refresh report list";
        }
    }

    protected override void Awake()
    {
        base.Awake();
        reportListGenerator = referenceManager.webBrowser.GetComponentInChildren<ReportListGenerator>();
    }

    public override void Click()
    {
        reportListGenerator.GenerateList();
    }

}
