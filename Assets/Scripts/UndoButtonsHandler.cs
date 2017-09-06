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

    void Start()
    {
        // There is no history when the program starts
        undoOneStepButton.SetButtonActive(false);
        redoOneStepButton.SetButtonActive(false);
        undoTenStepsButton.SetButtonActive(false);
        redoTenStepsButton.SetButtonActive(false);
    }

    /// <summary>
    /// Turns off the undo buttons.
    /// </summary>
    public void BeginningOfHistoryReached()
    {
        undoOneStepButton.SetButtonActive(false);
        undoTenStepsButton.SetButtonActive(false);
    }

    /// <summary>
    /// Turns on the undo buttons.
    /// </summary>
    public void BeginningOfHistoryLeft()
    {
        undoOneStepButton.SetButtonActive(true);
        undoTenStepsButton.SetButtonActive(true);
    }

    /// <summary>
    /// Turns off the redo buttons.
    /// </summary>
    public void EndOfHistoryReached()
    {
        redoOneStepButton.SetButtonActive(false);
        redoTenStepsButton.SetButtonActive(false);
    }

    /// <summary>
    /// Turns on the redo buttons.
    /// </summary>
    public void EndOfHistoryLeft()
    {
        redoOneStepButton.SetButtonActive(true);
        redoTenStepsButton.SetButtonActive(true);
    }
}

