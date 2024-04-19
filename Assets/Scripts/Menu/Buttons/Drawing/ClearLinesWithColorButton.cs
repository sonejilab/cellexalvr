using CellexalVR.Tools;
using UnityEngine;

namespace CellexalVR.Menu.Buttons.Drawing
{
    /// <summary>
    /// Represents the button that clears all lines drawn with a certain color with the draw tool.
    /// </summary>
    public class ClearLinesWithColorButton : CellexalButton
    {
        protected override string Description
        {
            get { return ""; }
        }

        public SpriteRenderer buttonRenderer;
        public Color color;

        private Color tintedColor;
        private DrawTool drawTool;

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            SetColors();
        }
#endif

        private void SetColors()
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
            SetColors();
        }

        public override void Click()
        {
            drawTool.SkipNextDraw();
            drawTool.ClearAllLinesWithColor(color);
            referenceManager.multiuserMessageSender.SendMessageClearAllLinesWithColor(color);
        }
    }
}