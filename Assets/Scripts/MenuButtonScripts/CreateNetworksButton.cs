/// <summary>
/// Represents the butotn that creates networks from a selection.
/// </summary>
public class CreateNetworksButton : CellexalButton
{
    private NetworkGenerator networkGenerator;
    private GameManager gameManager;

    protected override string Description
    {
        get
        {
            return "Create Networks";
        }
    }

    protected void Start()
    {

        networkGenerator = referenceManager.networkGenerator;
        gameManager = referenceManager.gameManager;
        SetButtonActivated(false);
        CellexalEvents.SelectionConfirmed.AddListener(TurnOn);
        CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    protected override void Click()
    {
        SetButtonActivated(false);
        networkGenerator.GenerateNetworks();
        gameManager.InformGenerateNetworks();
        CellexalEvents.NetworkCreated.Invoke();
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
