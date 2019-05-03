using CellexalVR.AnalysisLogic;
using CellexalVR.General;
using CellexalVR.Menu.SubMenus;

namespace CellexalVR.Menu.Buttons.Attributes
{
    /// <summary>
    /// Adds the points coloured according to the attributes to a new selection.
    /// </summary>
    public class SelectAllAttributesButton : CellexalButton
    {
        public AttributeSubMenu attributeSubMenu;
        public CloseMenuButton closeMenuButton;

        protected override string Description
        {
            get { return "Toggle All"; }
        }

        protected void Start()
        {
            SetButtonActivated(true);
            CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
            CellexalEvents.GraphsLoaded.AddListener(TurnOn);
        }

        public override void Click()
        {
            attributeSubMenu.SelectAllAttributes();
        }

        private void TurnOn()
        {
            SetButtonActivated(true);
        }

        private void TurnOff()
        {
            SetButtonActivated(false);
            spriteRenderer.sprite = deactivatedTexture;
        }
    }
}