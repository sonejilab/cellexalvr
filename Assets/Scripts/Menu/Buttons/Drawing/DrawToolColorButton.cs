using CellexalVR.Tools;
using UnityEngine;
namespace CellexalVR.Menu.Buttons.Drawing
{
    /// <summary>
    /// Represents the buttons that make up the color wheel for choosing the draw tool's color.
    /// </summary>
    public class DrawToolColorButton : CellexalButton
    {
        protected override string Description
        {
            get { return ""; }
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

        public override void Click()
        {
            drawTool.SkipNextDraw();
            drawTool.LineColor = color;
        }
    }
}