using UnityEngine;
/// <summary>
/// Represents the buttons that toggles individual categories on or off when flashing genes.
/// </summary>
public class FlashGenesCategoryButton : CellexalButton
{
    public TextMesh descriptionOnButton;

    public string Category { get; set; }
    public bool CategoryActivated { get; set; }

    protected override string Description
    {
        get { return "Toggle this category"; }
    }

    private CellManager cellManager;
    private bool buttonActive;

    protected void Start()
    {
        cellManager = referenceManager.cellManager;
        CellexalEvents.FlashGenesFileStartedLoading.AddListener(DeactivateButton);
        CellexalEvents.FlashGenesFileFinishedLoading.AddListener(ResetButton);
        CategoryActivated = true;
        descriptionOnButton.GetComponent<Renderer>().material.color = Color.green;
    }

    public override void Click()
    {
        CategoryActivated = !CategoryActivated;
        cellManager.FlashGenesCategoryFilter[Category] = CategoryActivated;
        if (CategoryActivated)
        {
            descriptionOnButton.GetComponent<Renderer>().material.color = Color.green;
        }
        else
        {
            descriptionOnButton.GetComponent<Renderer>().material.color = Color.red;
        }
    }

    private void ResetButton()
    {
        buttonActive = true;
        CategoryActivated = true;
        descriptionOnButton.GetComponent<Renderer>().material.color = Color.green;
    }

    private void DeactivateButton()
    {
        buttonActive = false;
    }
}
