using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Toggles graph information labels on/off.
/// </summary>
public class ShowLabelsButton : CellexalButton
{
    private GraphManager graphManager;
    private bool activate;

    void Start()
    {
        graphManager = referenceManager.graphManager;
        //GetComponent<SimpleTextRotator>().SetTransforms(this.transform, this.transform);
        activate = false;
    }

    protected override string Description
    {
        get
        {
             return "Show labels of object";
        }
    }

    protected override void Click()
    {
        graphManager.SetInfoPanelsVisible(activate);
        activate = !activate;
    }


}
