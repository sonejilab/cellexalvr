public class SaveNetworkAsTextFileButton : CellexalButton
{
    public NetworkCenter parent;

    protected override string Description
    {
        get { return "Save this network as a text file"; }
    }

    protected override void Click()
    {
        parent.SaveNetworkAsTextFile();
    }
}

