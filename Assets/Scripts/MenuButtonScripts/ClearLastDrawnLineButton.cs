using UnityEngine;

public class ClearLastDrawnLineButton : StationaryButton
{
    protected override string Description
    {
        get { return "Clear the last drawn line"; }
    }


    private Color tintedColor;
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
            drawTool.ClearLastLine();
        }
    }
}
