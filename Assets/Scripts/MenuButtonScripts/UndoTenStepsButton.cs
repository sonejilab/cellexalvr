using UnityEngine;

/// <summary>
/// This class represents the button that undoes the 10 last selected graphpoints.
/// </summary>
public class UndoTenStepsButton : StationaryButton
{

    public SelectionToolHandler selectionToolHandler;
    public Sprite grayScaleTexture;
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

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            for (int i = 0; i < 10; i++)
            {
                selectionToolHandler.GoBackOneStepInHistory();
            }
        }
    }

    public void SetButtonActive(bool active)
    {
        if (!active) controllerInside = false;
        buttonCollider.enabled = active;
        spriteRenderer.sprite = active ? standardTexture : grayScaleTexture;
    }
}
