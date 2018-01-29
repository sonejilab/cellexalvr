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
            cellManager.ColorGraphsByGene(gene, false);
        }
    }

    public void SetGene(string gene, float tValue)
    {
        this.gene = gene;
        description.text = string.Format("{0}\n{1:F3}", gene, tValue);
    }
}
