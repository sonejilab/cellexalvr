public class CreateNetworksButton : StationaryButton
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
        CellExAlEvents.SelectionConfirmed.AddListener(TurnOn);
        CellExAlEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            SetButtonActivated(false);
            networkGenerator.GenerateNetworks();
            gameManager.InformGenerateNetworks();
            CellExAlEvents.NetworkCreated.Invoke();
        }
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
