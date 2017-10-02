using UnityEngine;

/// <summary>
/// This class represents the button that redoes all the last undone graphpoints of the same color.
/// </summary>
public class RedoLastColorButton : StationaryButton
{
    public Sprite grayScaleTexture;

    private SelectionToolHandler selectionToolHandler;
    private Collider buttonCollider;

    protected override string Description
    {
        get { return "Redo last color"; }
    }

    private void Start()
    {
        selectionToolHandler = referenceManager.selectionToolHandler;
        SetButtonActivated(false);
        ButtonEvents.SelectionConfirmed.AddListener(TurnOff);
        ButtonEvents.SelectionCanceled.AddListener(TurnOff);
        ButtonEvents.EndOfHistoryReached.AddListener(TurnOff);
        ButtonEvents.EndOfHistoryLeft.AddListener(TurnOn);
        ButtonEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    protected override void Awake()
    {
        base.Awake();
        buttonCollider = gameObject.GetComponent<Collider>();
    }

    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            selectionToolHandler.GoForwardOneColorInHistory();
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