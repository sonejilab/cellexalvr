using UnityEngine;

/// <summary>
/// Represents the button that redoes all the last undone graphpoints of the same color.
/// </summary>
public class RedoLastColorButton : CellexalButton
{
    public Sprite grayScaleTexture;

    private SelectionToolHandler selectionToolHandler;

    protected override string Description
    {
        get { return "Redo last color"; }
    }

    private void Start()
    {
        selectionToolHandler = referenceManager.selectionToolHandler;
        SetButtonActivated(false);
        CellexalEvents.SelectionConfirmed.AddListener(TurnOff);
        CellexalEvents.SelectionCanceled.AddListener(TurnOff);
        CellexalEvents.EndOfHistoryReached.AddListener(TurnOff);
        CellexalEvents.EndOfHistoryLeft.AddListener(TurnOn);
        CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    protected override void Click()
    {
        referenceManager.gameManager.InformRedoOneColor();
        selectionToolHandler.GoForwardOneColorInHistory();
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