using System;

class DrawLinesBetweenGraphPointsButton : StationaryButton
{

    private CellManager cellManager;
    private SelectionToolHandler selectionToolHandler;

    protected override string Description
    {
        get { return "Draw lines between all cells with the same label in other graphs"; }
    }

    private void Start()
    {
        cellManager = referenceManager.cellManager;
        selectionToolHandler = referenceManager.selectionToolHandler;
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            cellManager.DrawLinesBetweenGraphPoints(selectionToolHandler.GetLastSelection());
        }
    }
}
