public class SaveNetworkAsImageButton : CellexalButton
{
    public NetworkCenter parent;

    protected override string Description
    {
        get { return "Save this network as an image"; }
    }

    protected override void Click()
    {
        parent.SaveNetworkAsImage();
    }
}

