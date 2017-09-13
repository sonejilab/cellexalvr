using UnityEngine;

/// <summary>
/// This class represents the button that undoes the 10 last selected graphpoints.
/// </summary>
public class UndoTenStepsButton : StationaryButton
{
    public Sprite grayScaleTexture;

    private SelectionToolHandler selectionToolHandler;
    private Collider buttonCollider;
    protected override string Description
    {
        get { return "Undo ten steps"; }
    }

    protected override void Awake()
    {
        base.Awake();
        buttonCollider = gameObject.GetComponent<Collider>();
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
            for (int i = 0; i < 10; i++)
            {
                selectionToolHandler.GoBackOneStepInHistory();
            }
        }
    }

    public override void SetButtonActivated(bool active)
    {
        base.SetButtonActivated(active);
        spriteRenderer.sprite = active ? standardTexture : grayScaleTexture;
    }
}
