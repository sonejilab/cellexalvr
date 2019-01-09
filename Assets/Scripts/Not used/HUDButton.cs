using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HUDButton : CellexalButton
{

    public GameObject HUD;
    public StatusDisplay status;
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
    void Start()
    {

    }

    // Update is called once per frame
    public override void Click()
    {
        HUD.SetActive(!HUD.activeSelf);
        status.ToggleStatusDisplay();
    }
}
