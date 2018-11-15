using UnityEngine;

/// <summary>
/// Represents the button that undoes the 10 last selected graphpoints.
/// </summary>
public class UndoTenStepsButton : CellexalButton
{
    public Sprite grayScaleTexture;

    private SelectionToolHandler selectionToolHandler;
    protected override string Description
    {
        get { return "Undo ten steps"; }
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

    protected override void Click()
    {
        referenceManager.gameManager.InformGoBackSteps(10);
        for (int i = 0; i < 10; i++)
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
