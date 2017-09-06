using UnityEngine;

public class UndoOneStepButton : StationaryButton
{

    public SelectionToolHandler selectionToolHandler;
    public Sprite grayScaleTexture;
    private Collider buttonCollider;
    protected override string Description
    {
        get { return "Undo one step"; }
    }

    protected override void Awake()
    {
        base.Awake();
        buttonCollider = gameObject.GetComponent<Collider>();
    }

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            selectionToolHandler.GoBackOneStepInHistory();
        }
    }

    public void SetButtonActive(bool active)
    {
        if (!active) controllerInside = false;
        buttonCollider.enabled = active;
        spriteRenderer.sprite = active ? standardTexture : grayScaleTexture;
    }
}
