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
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            drawTool.SkipNextDraw();
            drawTool.ClearAllLines();
        }
    }
}

