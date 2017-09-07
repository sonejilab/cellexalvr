﻿using UnityEngine;
/// <summary>
/// This class represents the button that redoes the 10 last undone graphpoints.
/// </summary>
public class RedoTenStepsButton : StationaryButton
{

    public SelectionToolHandler selectionToolHandler;
    public Sprite grayScaleTexture;
    private Collider buttonCollider;
    protected override string Description
    {
        get { return "Redo ten steps"; }
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
                selectionToolHandler.GoForwardOneStepInHistory();
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