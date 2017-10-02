using UnityEngine;

/// <summary>
/// This class represents the button that redoes the last undone graphpoint.
/// </summary>
public class RedoOneStepButton : StationaryButton
{
    public Sprite grayScaleTexture;

    private SelectionToolHandler selectionToolHandler;
    private Collider buttonCollider;
    protected override string Description
    {
        get { return "Redo one step"; }
    }

    protected override void Awake()
    {
        base.Awake();
        buttonCollider = gameObject.GetComponent<Collider>();
        SetButtonActivated(false);
        ButtonEvents.SelectionConfirmed.AddListener(TurnOff);
        ButtonEvents.SelectionCanceled.AddListener(TurnOff);
        ButtonEvents.EndOfHistoryReached.AddListener(TurnOff);
        ButtonEvents.EndOfHistoryLeft.AddListener(TurnOn);
        ButtonEvents.GraphsUnloaded.AddListener(TurnOff);

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
