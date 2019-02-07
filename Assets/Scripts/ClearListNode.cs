using System;
using UnityEngine;

public class ClearListNode : CellexalButton
{
    public DatasetList datasetList;
    public TextMesh textMesh;

    protected override string Description
    {
        get
        {
            return "";
        }
    }

    public override void Click()
    {
        datasetList.RemoveNode(textMesh.text);
        textMesh.text = "";
    }
}

