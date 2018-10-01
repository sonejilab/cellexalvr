using UnityEngine;
using VRTK;
/// <summary>
/// Represents the button for turning on and off the laser pointers.
/// </summary>
public class LasersButton : CellexalToolButton
{
    protected override void Awake()
    {
        base.Awake();
        TurnOn();
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
