using UnityEngine;

public class ColorByGeneButton : SolidButton
{

    public TextMesh description;

    private CellManager cellManager;
    private string gene;

    protected override void Start()
    {
        base.Start();
        cellManager = referenceManager.cellManager;
    }

    private void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            cellManager.ColorGraphsByGene(gene);
        }
    }

    public void SetGene(string gene)
    {
        this.gene = gene;
        description.text = gene;
    }
}

