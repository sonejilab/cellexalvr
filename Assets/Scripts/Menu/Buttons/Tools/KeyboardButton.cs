using CellexalVR.General;
using CellexalVR.Interaction;
using UnityEngine;
namespace CellexalVR.Menu.Buttons.Tools
{
    ///<summary>
    /// Represents a button used for toggling the keyboard.
    ///</summary>
    public class KeyboardButton : CellexalToolButton
    {
        protected override string Description
        {
            get { return "Toggle keyboard"; }
        }

        protected override ControllerModelSwitcher.Model ControllerModel
        {
            get { return ControllerModelSwitcher.Model.Keyboard; }
        }

        public override void Click()
        {
            base.Click();
            referenceManager.multiuserMessageSender.SendMessageActivateKeyboard(toolActivated);
            CellexalEvents.KeyboardToggled.Invoke();
        }
    }
}
