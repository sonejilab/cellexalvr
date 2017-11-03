using UnityEngine;

public class ClearAllDrawnLinesWithAColor : StationaryButton
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
        tintedColor = new Color(oldColor.r - oldColor.r / 2f, oldColor.g - oldColor.g / 2f, oldColor.b - oldColor.b / 2f);
    }

    private void Start()
    {
        drawTool = referenceManager.drawTool;
        buttonRenderer = GetComponent<SpriteRenderer>();
        buttonRenderer.color = color;
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

    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        if (controllerInside)
        {
            buttonRenderer.color = tintedColor;
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
        if (!controllerInside)
        {
            buttonRenderer.color = color;
        }
    }
}
