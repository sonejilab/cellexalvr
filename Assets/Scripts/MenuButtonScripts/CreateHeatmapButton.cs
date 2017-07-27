///<summary>
/// This class represents a button used for creating a heatmap from a cell selection.
///</summary>
public class CreateHeatmapButton : RotatableButton
{
    protected override string Description
    {
        get { return "Create heatmap"; }
    }
    public HeatmapGenerator heatmapGenerator;

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) && !isRotating)
        {
            SetButtonState(false);
            heatmapGenerator.CreateHeatmap();
        }
    }

}