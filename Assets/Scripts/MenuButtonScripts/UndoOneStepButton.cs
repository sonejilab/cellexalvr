using UnityEngine;

/// <summary>
/// Represents the button that undoes the last selected graphpoint.
/// </summary>
public class UndoOneStepButton : CellexalButton
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
        CellexalEvents.SelectionStarted.AddListener(TurnOn);
        CellexalEvents.SelectionConfirmed.AddListener(TurnOff);
        CellexalEvents.SelectionCanceled.AddListener(TurnOff);
        CellexalEvents.BeginningOfHistoryReached.AddListener(TurnOff);
        CellexalEvents.BeginningOfHistoryLeft.AddListener(TurnOn);
        CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
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
