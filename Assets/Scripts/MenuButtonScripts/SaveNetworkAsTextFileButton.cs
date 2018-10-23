public class SaveNetworkAsTextFileButton : CellexalButton
{
    public NetworkCenter parent;

    protected override string Description
    {
        get { return "Save this network as a text file"; }
    }

    protected override void Awake()
    {
        base.Awake();
        CellexalEvents.NetworkEnlarged.AddListener(TurnOn);
        CellexalEvents.NetworkUnEnlarged.AddListener(TurnOff);
        TurnOff();
    }

    protected override void Click()
    {
        parent.SaveNetworkAsTextFile();
        device.TriggerHapticPulse(2000);
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

