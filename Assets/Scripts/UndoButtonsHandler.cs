using UnityEngine;

/// <summary>
/// This class is responsible for knowing which undo and redo buttons should be turned on or off.
/// </summary>
public class UndoButtonsHandler : MonoBehaviour
{
    public UndoOneStepButton undoOneStepButton;
    public RedoOneStepButton redoOneStepButton;
    public UndoTenStepsButton undoTenStepsButton;
    public RedoTenStepsButton redoTenStepsButton;
    public UndoLastColorButton undoLastColorButton;
    public RedoLastColorButton redoLastColorButton;

    void Start()
    {
        // There is no history when the program starts
        TurnAllButtonsOff();
    }

    public void TurnAllButtonsOff()
    {
        undoOneStepButton.SetButtonActive(false);
        redoOneStepButton.SetButtonActive(false);
        undoTenStepsButton.SetButtonActive(false);
        redoTenStepsButton.SetButtonActive(false);
        undoLastColorButton.SetButtonActive(false);
        redoLastColorButton.SetButtonActive(false);
    }

    /// <summary>
    /// Turns off the undo buttons.
    /// </summary>
    public void BeginningOfHistoryReached()
    {
        undoOneStepButton.SetButtonActive(false);
        undoTenStepsButton.SetButtonActive(false);
        undoLastColorButton.SetButtonActive(false);
    }

    /// <summary>
    /// Turns on the undo buttons.
    /// </summary>
    public void BeginningOfHistoryLeft()
    {
        undoOneStepButton.SetButtonActive(true);
        undoTenStepsButton.SetButtonActive(true);
        undoLastColorButton.SetButtonActive(true);
    }

    /// <summary>
    /// Turns off the redo buttons.
    /// </summary>
    public void EndOfHistoryReached()
    {
        redoOneStepButton.SetButtonActive(false);
        redoTenStepsButton.SetButtonActive(false);
        redoLastColorButton.SetButtonActive(false);
    }

    /// <summary>
    /// Turns on the redo buttons.
    /// </summary>
    public void EndOfHistoryLeft()
    {
        redoOneStepButton.SetButtonActive(true);
        redoTenStepsButton.SetButtonActive(true);
        redoLastColorButton.SetButtonActive(true);
    }
}

