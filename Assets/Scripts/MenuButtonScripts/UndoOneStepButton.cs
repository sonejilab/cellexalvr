using UnityEngine;

/// <summary>
/// This class represents the button that undoes the last selected graphpoint.
/// </summary>
public class UndoOneStepButton : StationaryButton
{
    public Sprite grayScaleTexture;

    private SelectionToolHandler selectionToolHandler;
    protected override string Description
    {
        get { return "Undo one step"; }
    }

    protected override void Awake()
    {
        base.Awake();
        SetButtonActivated(false);
        CellExAlEvents.SelectionStarted.AddListener(TurnOn);
        CellExAlEvents.SelectionConfirmed.AddListener(TurnOff);
        CellExAlEvents.SelectionCanceled.AddListener(TurnOff);
        CellExAlEvents.BeginningOfHistoryReached.AddListener(TurnOff);
        CellExAlEvents.BeginningOfHistoryLeft.AddListener(TurnOn);
        CellExAlEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    private void Start()
    {
        selectionToolHandler = referenceManager.selectionToolHandler;
    }

    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            selectionToolHandler.GoBackOneStepInHistory();
        }
    }

    private void TurnOff()
    {
        SetButtonActivated(false);
    }

    private void TurnOn()
    {
        SetButtonActivated(true);
    }
}
