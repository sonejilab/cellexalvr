
class RecolorGraphsAfterLastSelectionButton : StationaryButton
{
    private SelectionToolHandler selectionToolHandler;
    private GraphManager graphManager;

    protected override string Description
    {
        get
        {
            return "Colors the selected cells in all other graphs";
        }
    }

    private void Start()
    {
        selectionToolHandler = referenceManager.selectionToolHandler;
        graphManager = referenceManager.graphManager;
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            graphManager.RecolorAllGraphsAfterSelection();
        }
    }
}
