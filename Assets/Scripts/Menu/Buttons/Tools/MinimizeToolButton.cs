using CellexalVR.Interaction;
namespace CellexalVR.Menu.Buttons.Tools
{
    /// <summary>
    /// Represents the buttont that minimizes things
    /// </summary>
    class MinimizeToolButton : CellexalToolButton
    {
        private bool changeSprite;

        protected override string Description
        {
            get { return "Toggle minimizer tool"; }
        }

        protected override ControllerModelSwitcher.Model ControllerModel
        {
            get { return ControllerModelSwitcher.Model.Minimizer; }
        }

        public override void SetHighlighted(bool highlight)
        {
            base.SetHighlighted(highlight);
            //infoMenu.SetActive(highlight);
        }


    }
}