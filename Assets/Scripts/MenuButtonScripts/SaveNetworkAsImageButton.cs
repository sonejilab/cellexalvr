public class SaveNetworkAsImageButton : CellexalButton
{
    public NetworkCenter parent;

    protected override string Description
    {
        get { return "Save this network as an image"; }
    }

    protected override void Awake()
    {
        base.Awake();
        //CellexalEvents.NetworkEnlarged.AddListener(TurnOn);
        //CellexalEvents.NetworkUnEnlarged.AddListener(TurnOff);
        TurnOff();
    }

    protected override void Click()
    {
        parent.SaveNetworkAsImage();
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

