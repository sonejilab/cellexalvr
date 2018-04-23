/// <summary>
/// Represents the button that clears all lines drawn with the draw tool.
/// </summary>
public class ClearAllDrawToolLinesButton : CellexalButton
{
    protected override string Description
    {
        get { return "Toggles the draw tool"; }
    }

    private DrawTool drawTool;

    private void Start()
    {
        drawTool = referenceManager.drawTool;
    }

    protected override void Click()
    {
        drawTool.SkipNextDraw();
        drawTool.ClearAllLines();

    }
}

