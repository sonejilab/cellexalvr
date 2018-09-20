using UnityEngine;
/// <summary>
/// Represents the buttont that minimizes things
/// </summary>
class MinimizeToolButton : CellexalToolButton
{
    private bool changeSprite;
    public GameObject infoMenu;

    protected override string Description
    {
        get { return "Toggle minimizer tool"; }
    }

    protected override ControllerModelSwitcher.Model ControllerModel
    {
        get { return ControllerModelSwitcher.Model.Minimizer; }
    }

    //public override void SetHighlighted(bool highlight)
    //{
    //    base.SetHighlighted(highlight);
    //    infoMenu.SetActive(highlight);
    //}
}
