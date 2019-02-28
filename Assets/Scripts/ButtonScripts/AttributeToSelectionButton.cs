using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class AttributeToSelectionButton : CellexalButton
{
    private CellManager cellManager;
    public CloseMenuButton closeMenuButton;
    public SubMenuButton selectionToolButton;

    protected override string Description
    {
        get { return "Go to Selection Menu"; }
    }

    protected void Start()
    {
        cellManager = referenceManager.cellManager;
        SetButtonActivated(true);
        CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
        CellexalEvents.GraphsLoaded.AddListener(TurnOn);
    }

    public override void Click()
    {
        cellManager.SendToSelection();
        referenceManager.menuRotator.RotateLeft(2);
        closeMenuButton.CloseMenu();
        selectionToolButton.OpenMenu();
        //TurnOff();
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

