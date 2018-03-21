using UnityEngine;

/// <summary>
/// Represents the button that clears all lines drawn with a certain color with the draw tool.
/// </summary>
public class ClearAllDrawnLinesWithAColor : CellexalButton
{
    protected override string Description
    {
        get { return "Change the color of the draw tool"; }
    }

    public SpriteRenderer buttonRenderer;
    public Color color;

    private Color tintedColor;
    private DrawTool drawTool;

    private void OnValidate()
    {
        buttonRenderer.color = color;
        Color oldColor = buttonRenderer.color;
        float newr = oldColor.r - oldColor.r / 2f;
        float newg = oldColor.g - oldColor.g / 2f;
        float newb = oldColor.b - oldColor.b / 2f;
        tintedColor = new Color(newr, newg, newb);
    }

    private void Start()
    {
        drawTool = referenceManager.drawTool;
        buttonRenderer = GetComponent<SpriteRenderer>();
        OnValidate();
    }

    private void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            drawTool.SkipNextDraw();
            drawTool.ClearAllLinesWithColor(color);
        }
    }
}
