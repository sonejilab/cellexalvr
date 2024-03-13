using CellexalVR.General;
using CellexalVR.Interaction;

namespace CellexalVR.Menu.Buttons.Tools
{
    /// <summary>
    /// Represents the button for turning on and off the laser pointers.
    /// </summary>
    public class LasersButton : CellexalToolButton
    {
        protected override void Awake()
        {
            base.Awake();
            TurnOn();
            CellexalEvents.GraphsUnloaded.RemoveListener(TurnOff);
        }
        protected override string Description
        {
            get { return "Toggle Lasers"; }
        }
        protected override ControllerModelSwitcher.Model ControllerModel
        {
            get { return ControllerModelSwitcher.Model.TwoLasers; }
        }
    }
}