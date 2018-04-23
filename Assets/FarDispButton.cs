using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarDispButton : CellexalButton
{

    public GameObject FarDisp;
    public StatusDisplay status;

    protected override string Description
    {
        get { return "Toggle Far Away Display"; }
    }
    protected override void Awake()
    {
        base.Awake();

        //controllerModelSwitcher = referenceManager.controllerModelSwitcher;
    }
    void Start()
    {

    }

    protected override void Click()
    {
        FarDisp.SetActive(!FarDisp.activeSelf);
        status.ToggleStatusDisplay();

    }

}

