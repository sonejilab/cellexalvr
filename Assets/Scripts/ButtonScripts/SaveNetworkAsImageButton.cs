/// <summary>
/// For reporting function. Network is saved as image to the user specific folder. If user wants to create a report
/// the image is included in it.
/// </summary>
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

