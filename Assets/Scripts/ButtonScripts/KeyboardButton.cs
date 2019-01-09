using UnityEngine;
///<summary>
/// Represents a button used for toggling the keyboard.
///</summary>
public class KeyboardButton : CellexalToolButton
{
    private bool activateKeyboard = false;

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
        referenceManager.gameManager.InformActivateKeyboard(toolActivated);
    }



}
