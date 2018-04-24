
public class SwitchAttributeButtonMode : CellexalButton
{
    protected override string Description
    {
        get { return "Switch the modes of the attribute coloring"; }
    }

    protected AttributeSubMenu attributeMenu;

    private void Start()
    {
        attributeMenu = referenceManager.attributeSubMenu;
    }

    protected override void Click()
    {
        attributeMenu.SwitchButtonStates();
    }
}
