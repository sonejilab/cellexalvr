using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarDispButton : StationaryButton
{

    public GameObject FarDisp;
    protected override string Description
    {
        get { return "Toggle Far Away Display"; }
    }
    protected override void Awake()
    {
        base.Awake();

        //controllerModelSwitcher = referenceManager.controllerModelSwitcher;
    }
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            FarDisp.SetActive(!FarDisp.activeSelf);

        }

    }
}
