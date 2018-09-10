using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowLabelsButton : CellexalButton
{

    private ReferenceManager referenceManager;
    private GraphManager graphManager;

    void Start()
    {
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        graphManager = referenceManager.graphManager;
    }

    protected override string Description
    {
        get
        {
             return "Show labels of objects";
        }
    }

    protected override void Click()
    {
        graphManager.SetInfoPanelsVisible(true);
    }


}
