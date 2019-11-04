using CellexalVR.Tools;
namespace CellexalVR.Menu.Buttons.Drawing
{
    /// <summary>
    /// Represents the button that clears all lines drawn with the draw tool.
    /// </summary>
    public class ClearAllLinesButton : CellexalButton
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
            referenceManager.multiuserMessageSender.SendMessageClearAllLines();

        }
    }
}