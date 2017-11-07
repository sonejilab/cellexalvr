using System;

class ClearLinesBetweenGraphPointsButton : StationaryButton
{

    private CellManager cellManager;

    protected override string Description
    {
        get { return "Draw lines between all cells with the same label in other graphs"; }
    }

    private void Start()
    {
        cellManager = referenceManager.cellManager;
        SetButtonActivated(false);
        CellExAlEvents.LinesBetweenGraphsDrawn.AddListener(TurnOn);
    }

    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            cellManager.ClearLinesBetweenGraphPoints();
            CellExAlEvents.LinesBetweenGraphsCleared.Invoke();
            SetButtonActivated(false);
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
