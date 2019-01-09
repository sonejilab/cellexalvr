
public class SwitchAttributeButtonMode : CellexalButton
{
    protected override string Description
    {
        get { return "Switch button appearance"; }
    }

    protected AttributeSubMenu attributeMenu;

    private void Start()
    {
        attributeMenu = referenceManager.attributeSubMenu;
    }

    public override void Click()
    {
        attributeMenu.SwitchButtonStates();
    }
}
