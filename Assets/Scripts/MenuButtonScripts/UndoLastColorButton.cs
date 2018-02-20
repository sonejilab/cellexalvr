using UnityEngine;

/// <summary>
/// Represents the button that undoes all the last selected graphpoints of the same color.
/// </summary>
public class UndoLastColorButton : StationaryButton
{
    public Sprite grayScaleTexture;

    private SelectionToolHandler selectionToolHandler;
    protected override string Description
    {
        get { return "Undo last color"; }
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
            selectionToolHandler.GoBackOneColorInHistory();
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