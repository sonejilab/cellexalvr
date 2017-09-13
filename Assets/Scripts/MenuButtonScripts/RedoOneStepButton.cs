using UnityEngine;

/// <summary>
/// This class represents the button that redoes the last undone graphpoint.
/// </summary>
public class RedoOneStepButton : StationaryButton
{

    public SelectionToolHandler selectionToolHandler;
    public Sprite grayScaleTexture;
    private Collider buttonCollider;
    protected override string Description
    {
        get { return "Redo one step"; }
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
            selectionToolHandler.GoForwardOneStepInHistory();
        }
    }

    public override void SetButtonActivated(bool active)
    {
        base.SetButtonActivated(active);
        spriteRenderer.sprite = active ? standardTexture : grayScaleTexture;
    }
}
