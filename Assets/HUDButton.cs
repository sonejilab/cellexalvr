using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDButton : StationaryButton {

    public GameObject HUD;
    protected override string Description
    {
        get { return "Toggle HUD"; }
    }
    protected override void Awake()
    {
        base.Awake();
        
        //controllerModelSwitcher = referenceManager.controllerModelSwitcher;
    }
    // Use this for initialization
    void Start () {
		
	}

    // Update is called once per frame
    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            HUD.SetActive(!HUD.activeSelf);

        }

    }
}
