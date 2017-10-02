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
        SetButtonActivated(false);
        ButtonEvents.SelectionConfirmed.AddListener(TurnOn);
        ButtonEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            cellManager.DrawLinesBetweenGraphPoints(selectionToolHandler.GetLastSelection());
            ButtonEvents.LinesBetweenGraphsDrawn.Invoke();
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
