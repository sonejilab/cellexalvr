public class CreateNetworksButton : RotatableButton
{
    private NetworkGenerator networkGenerator;
    private MenuRotator rotator;

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
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            SetButtonState(false);
            networkGenerator.GenerateNetworks();
            if (rotator.gameObject.activeSelf && rotator.SideFacingPlayer == MenuRotator.Rotation.Right)
            {
                rotator.RotateLeft();
            }
        }
    }
}


