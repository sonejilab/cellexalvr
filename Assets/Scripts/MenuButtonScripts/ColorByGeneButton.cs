using UnityEngine;

/// <summary>
/// Represents a button that can be pressed to color all graphs based on the expression of some gene.
/// </summary>
public class ColorByGeneButton : CellexalButton
{

    public TextMesh description;

    private CellManager cellManager;
    private string gene;

    protected override string Description
    {
        get { return "Color all graphs based on a gene expression"; }
    }

    protected void Start()
    {
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

    /// <summary>
    /// Sets the text on this button.
    /// </summary>
    /// <param name="gene">The name of the gene</param>
    /// <param name="value">The value that this gene was sorted by.</param>
    public void SetGene(string gene, float value)
    {
        this.gene = gene;
        description.text = string.Format("{0}\n{1:F3}", gene, value);
    }
}
