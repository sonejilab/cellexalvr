/// <summary>
/// Represents the button that clears all lines drawn with the draw tool.
/// </summary>
public class ClearAllDrawToolLinesButton : StationaryButton
{
    protected override string Description
    {
        get { return "Toggles the draw tool"; }
    }

    private DrawTool drawTool;

    private void Start()
    {
        drawTool = referenceManager.drawTool;
    }

    private void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            drawTool.SkipNextDraw();
            drawTool.ClearAllLines();
        }
    }
}

