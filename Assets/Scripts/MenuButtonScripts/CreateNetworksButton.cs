public class CreateNetworksButton : StationaryButton
{
    private NetworkGenerator networkGenerator;
    private MenuRotator rotator;
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
        rotator = referenceManager.menuRotator;
        gameManager = referenceManager.gameManager;
        SetButtonActivated(false);
        ButtonEvents.SelectionConfirmed.AddListener(TurnOn);
        ButtonEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            SetButtonActivated(false);
            networkGenerator.GenerateNetworks();
            gameManager.InformGenerateNetworks();
            ButtonEvents.NetworkCreated.Invoke();
            //if (rotator.gameObject.activeSelf && rotator.SideFacingPlayer == MenuRotator.Rotation.Right)
            //{
            //    rotator.RotateLeft();
            //}
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
