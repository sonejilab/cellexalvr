using CellexalVR.General;
using CellexalVR.Interaction;

namespace CellexalVR.Menu.Buttons.Selection
{
    ///<summary>
    /// Represents a button used for toggling the selection tool.
    ///</summary>
    public class SelectionToolButton : CellexalToolButton
    {
        protected override string Description
        {
            get { return "Toggle selection tool"; }
        }

        protected override void Awake()
        {
            base.Awake();
            CellexalEvents.NetworkCreated.AddListener(TurnOn);
        }

        protected override ControllerModelSwitcher.Model ControllerModel
        {
            get { return ControllerModelSwitcher.Model.SelectionTool; }
        }

    }
}