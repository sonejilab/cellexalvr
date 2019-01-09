using UnityEngine;
/// <summary>
/// Represent the button that clears the last line drawn with the draw tool.
/// </summary>
public class ClearLastDrawnLineButton : CellexalButton
{
    protected override string Description
    {
        get { return "Clear the last drawn line"; }
    }


    private Color tintedColor;
    private DrawTool drawTool;

    private void Start()
    {
        drawTool = referenceManager.drawTool;
    }

    public override void Click()
    {
        drawTool.SkipNextDraw();
        drawTool.ClearLastLine();
        referenceManager.gameManager.InformClearLastLine();
    }
}
