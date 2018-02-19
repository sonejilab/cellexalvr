
class CreateNewHeatmapButton : StationaryButton
{
    protected override string Description
    {
        get { return "Create a new heatmap from your selection"; }
    }

    private void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            GetComponentInParent<Heatmap>().CreateNewHeatmapFromSelection();
        }
    }
}

