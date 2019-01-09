using UnityEngine;
/// <summary>
/// Represents the button that closes the index menu.
/// </summary>
public class CloseIndexMenuButton : CellexalButton
{
    private GameObject indexMenu;
    private GameObject buttons;

    protected override string Description
    {
        get
        {
            return "Close index menu";
        }
    }

    private void Start()
    {
        indexMenu = referenceManager.indexMenu.gameObject;
        buttons = referenceManager.leftButtons;
    }

    public override void Click()
    {
        spriteRenderer.sprite = standardTexture;
        controllerInside = false;
        indexMenu.SetActive(false);
        buttons.SetActive(true);
    }
}

