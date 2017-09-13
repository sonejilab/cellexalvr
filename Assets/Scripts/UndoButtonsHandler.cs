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
        undoOneStepButton.SetButtonActivated(false);
        redoOneStepButton.SetButtonActivated(false);
        undoTenStepsButton.SetButtonActivated(false);
        redoTenStepsButton.SetButtonActivated(false);
        undoLastColorButton.SetButtonActivated(false);
        redoLastColorButton.SetButtonActivated(false);
    }

    /// <summary>
    /// Turns off the undo buttons.
    /// </summary>
    public void BeginningOfHistoryReached()
    {
        undoOneStepButton.SetButtonActivated(false);
        undoTenStepsButton.SetButtonActivated(false);
        undoLastColorButton.SetButtonActivated(false);
    }

    /// <summary>
    /// Turns on the undo buttons.
    /// </summary>
    public void BeginningOfHistoryLeft()
    {
        undoOneStepButton.SetButtonActivated(true);
        undoTenStepsButton.SetButtonActivated(true);
        undoLastColorButton.SetButtonActivated(true);
    }

    /// <summary>
    /// Turns off the redo buttons.
    /// </summary>
    public void EndOfHistoryReached()
    {
        redoOneStepButton.SetButtonActivated(false);
        redoTenStepsButton.SetButtonActivated(false);
        redoLastColorButton.SetButtonActivated(false);
    }

    /// <summary>
    /// Turns on the redo buttons.
    /// </summary>
    public void EndOfHistoryLeft()
    {
        redoOneStepButton.SetButtonActivated(true);
        redoTenStepsButton.SetButtonActivated(true);
        redoLastColorButton.SetButtonActivated(true);
    }
}

