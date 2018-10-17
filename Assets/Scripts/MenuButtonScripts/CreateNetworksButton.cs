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
        var rand = new System.Random();
        var layoutSeed = rand.Next();
        print(layoutSeed);
        networkGenerator.GenerateNetworks(layoutSeed);
        gameManager.InformGenerateNetworks(layoutSeed);
        CellexalEvents.NetworkCreated.Invoke();
        SetButtonActivated(false);
        Exit();
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
