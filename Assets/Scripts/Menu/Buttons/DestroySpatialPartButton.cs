using UnityEngine;
using CellexalVR.Menu.Buttons;
using TMPro;
using CellexalVR.Spatial;

public class DestroySpatialPartButton: CellexalButton
{

    private string modelName;

    private void Start()
    {
        modelName = GetComponentInParent<BrainPartButton>().ModelName;
    }


    protected override string Description => $"Destroy {modelName} mesh from reference";

    public override void Click()
    {
        GetComponentInParent<AllenReferenceBrain>().RemovePart(modelName);
    }

}

