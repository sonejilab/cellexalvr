public class CreateNetworksButton : RotatableButton
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

    protected override void Start()
    {
        base.Start();
        networkGenerator = referenceManager.networkGenerator;
        rotator = referenceManager.menuRotator;
        gameManager = referenceManager.gameManager;
        SetButtonState(false);
        ButtonEvents.SelectionConfirmed.AddListener(TurnOn);
        ButtonEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            SetButtonState(false);
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
        SetButtonState(true);
    }

    private void TurnOff()
    {
        SetButtonState(false);
    }
}


