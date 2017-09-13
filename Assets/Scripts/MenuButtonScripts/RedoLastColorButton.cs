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

    public override void SetButtonActivated(bool active)
    {
        base.SetButtonActivated(active);
        spriteRenderer.sprite = active ? standardTexture : grayScaleTexture;
    }
}