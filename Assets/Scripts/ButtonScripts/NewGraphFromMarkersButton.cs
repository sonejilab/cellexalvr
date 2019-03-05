using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewGraphFromMarkersButton : CellexalButton
{
    private string indexName;

    protected override string Description
    {
        get { return "Create new Graph"; }
    }

    protected void Start()
    {
    }

    public override void Click()
    { 
        referenceManager.newGraphFromMarkers.CreateMarkerGraph();
    }

}

