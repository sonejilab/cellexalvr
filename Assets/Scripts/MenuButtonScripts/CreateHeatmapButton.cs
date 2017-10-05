///<summary>
/// This class represents a button used for creating a heatmap from a cell selection.
///</summary>
public class CreateHeatmapButton : StationaryButton
{

    private HeatmapGenerator heatmapGenerator;
    private GameManager gameManager;


    protected override string Description
    {
        get { return "Create heatmap"; }
    }

    protected void Start()
    {
        heatmapGenerator = referenceManager.heatmapGenerator;
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
            heatmapGenerator.CreateHeatmap();
            gameManager.InformCreateHeatmap();
            ButtonEvents.HeatmapCreated.Invoke();
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
