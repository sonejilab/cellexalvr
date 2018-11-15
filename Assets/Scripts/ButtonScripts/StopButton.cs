/// <summary>
/// Represents the button that stops the flashing genes.
/// </summary>

class StopButton : CellexalButton
{
    public CellManager.FlashGenesMode switchToMode;
    public StationaryButton random;
    public StationaryButton shuffle;
    public StationaryButton ordered;

    private CellManager cellManager;

    protected override string Description
    {
        get { return "Change the mode"; }
    }


    private void Start()
    {
        cellManager = referenceManager.cellManager;
    }
    // Use this for initialization


    // Update is called once per frame
    protected override void Click()
    {
        cellManager.CurrentFlashGenesMode = switchToMode;
        random.spriteRenderer.sprite = random.deactivatedTexture;
        shuffle.spriteRenderer.sprite = shuffle.deactivatedTexture;
        ordered.spriteRenderer.sprite = ordered.deactivatedTexture;
        spriteRenderer.sprite = deactivatedTexture;
        //SetButtonActivated(false);
    }
}
