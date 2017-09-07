using UnityEngine;

/// <summary>
/// This class represents the button that undoes all the last selected graphpoints of the same color.
/// </summary>
public class UndoLastColorButton : StationaryButton
{

    public SelectionToolHandler selectionToolHandler;
    public Sprite grayScaleTexture;
    private Collider buttonCollider;
    protected override string Description
    {
        get { return "Undo last color"; }
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
            selectionToolHandler.GoBackOneColorInHistory();
        }
    }

    public void SetButtonActive(bool active)
    {
        if (!active) controllerInside = false;
        buttonCollider.enabled = active;
        spriteRenderer.sprite = active ? standardTexture : grayScaleTexture;
    }
}