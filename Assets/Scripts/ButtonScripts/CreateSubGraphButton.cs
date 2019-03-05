public class CreateSubGraphButton : CellexalButton
{

    private CombinedGraphGenerator combinedGraphGenerator;
    private GameManager gameManager;

    protected override string Description
    {
        get { return "Create Sub Graph"; }
    }

    protected void Start()
    {

        combinedGraphGenerator = referenceManager.combinedGraphGenerator;
        gameManager = referenceManager.gameManager;
    }

    public override void Click()
    {
        combinedGraphGenerator.CreateSubGraphs(referenceManager.attributeSubMenu.attributes);
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
