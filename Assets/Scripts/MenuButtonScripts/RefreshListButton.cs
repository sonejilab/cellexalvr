using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefreshListButton : CellexalButton
{
    private ReportListGenerator reportListGenerator;

    protected override string Description
    {
        get
        {
            return "Refresh report list";
        }
    }

    protected override void Awake()
    {
        base.Awake();
        reportListGenerator = referenceManager.webBrowser.GetComponentInChildren<ReportListGenerator>();
    }

    protected override void Click()
    {
        reportListGenerator.GenerateList();
    }

}
