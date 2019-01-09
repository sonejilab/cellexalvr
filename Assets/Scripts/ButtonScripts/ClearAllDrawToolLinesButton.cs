/// <summary>
/// Represents the button that clears all lines drawn with the draw tool.
/// </summary>
public class ClearAllDrawToolLinesButton : CellexalButton
{
    protected override string Description
    {
        get { return "Clear all lines"; }
    }

    private DrawTool drawTool;

    private void Start()
    {
        drawTool = referenceManager.drawTool;
    }

    public override void Click()
    {
        drawTool.SkipNextDraw();
        drawTool.ClearAllLines();
        referenceManager.gameManager.InformClearAllLines();

    }
}

