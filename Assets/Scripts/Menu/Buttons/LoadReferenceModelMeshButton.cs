using UnityEngine;
using System.Collections;
using CellexalVR.Menu.Buttons;
using TMPro;

public class LoadReferenceModelMeshButton : CellexalButton
{
    public TextMeshPro nameHeader;
    private string modelName;

    public string ModelName
    {
        get => modelName;
        set
        {
            modelName = value;
            nameHeader.text = value;
        }
    }

    protected override string Description => $"Load {modelName} mesh from reference";

    public override void Click()
    {
        AllenReferenceBrain.instance.SpawnModel(modelName);
    }

}
