
/// <summary>
/// Represents the button that calculates the correlated genes.
/// </summary>
public class CorrelatedGenesButton : ClickablePanel
{

    public PreviousSearchesListNode listNode;

    private CorrelatedGenesList correlatedGenesList;

    protected override void Start()
    {
        correlatedGenesList = referenceManager.correlatedGenesList;
    }

    public override void Click()
    {
        // the gene name is followed by some other text
        correlatedGenesList.CalculateCorrelatedGenes(listNode.NameOfThing, listNode.Type);
    }
}
