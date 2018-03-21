using UnityEngine;

/// <summary>
/// Represents the button that redoes the last undone graphpoint.
/// </summary>
public class RedoOneStepButton : CellexalButton
{
    public Sprite grayScaleTexture;

    private SelectionToolHandler selectionToolHandler;
    protected override string Description
    {
        get { return "Redo one step"; }
    }

    protected override void Awake()
    {
        base.Awake();
        SetButtonActivated(false);
        CellexalEvents.SelectionConfirmed.AddListener(TurnOff);
        CellexalEvents.SelectionCanceled.AddListener(TurnOff);
        CellexalEvents.EndOfHistoryReached.AddListener(TurnOff);
        CellexalEvents.EndOfHistoryLeft.AddListener(TurnOn);
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
            selectionToolHandler.GoForwardOneStepInHistory();
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
