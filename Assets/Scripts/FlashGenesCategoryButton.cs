using UnityEngine;
/// <summary>
/// This class represents the buttons that toggles individual categories on or off when flashing genes.
/// </summary>
public class FlashGenesCategoryButton : SolidButton
{
    public TextMesh textRenderer;

    public string Category { get; set; }
    public bool CategoryActivated { get; set; }

    private CellManager cellManager;
    private bool buttonActive;

    protected override void Start()
    {
        base.Start();
        cellManager = referenceManager.cellManager;
        CellExAlEvents.FlashGenesFileStartedLoading.AddListener(DeactivateButton);
        CellExAlEvents.FlashGenesFileFinishedLoading.AddListener(ResetButton);
        CategoryActivated = true;
        textRenderer.GetComponent<Renderer>().material.color = Color.green;
    }

    protected void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger) && buttonActive)
        {
            CategoryActivated = !CategoryActivated;
            cellManager.FlashGenesCategoryFilter[Category] = CategoryActivated;
            if (CategoryActivated)
            {
                textRenderer.GetComponent<Renderer>().material.color = Color.green;
            }
            else
            {
                textRenderer.GetComponent<Renderer>().material.color = Color.red;
            }
        }
    }

    private void ResetButton()
    {
        buttonActive = true;
        CategoryActivated = true;
        textRenderer.GetComponent<Renderer>().material.color = Color.green;
    }

    private void DeactivateButton()
    {
        buttonActive = false;
    }
}
