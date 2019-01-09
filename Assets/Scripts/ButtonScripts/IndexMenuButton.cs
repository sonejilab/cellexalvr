using UnityEngine;
/// <summary>
/// Represents the button that opens the color by index menu.
/// </summary>
public class IndexMenuButton : CellexalButton
{
    private GameObject indexMenu;
    private GameObject buttons;

    protected override string Description
    {
        get { return "Show menu for coloring by index"; }
    }
    protected override void Awake()
    {
        base.Awake();
        CellexalEvents.GraphsLoaded.AddListener(TurnOn);
        CellexalEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    void Start()
    {
        indexMenu = referenceManager.indexMenu.gameObject;
        buttons = referenceManager.leftButtons;
        SetButtonActivated(false);
    }

    public override void Click()
    {
        spriteRenderer.sprite = standardTexture;
        descriptionText.text = "";
        controllerInside = false;
        indexMenu.SetActive(true);
        buttons.SetActive(false);
    }

    private void TurnOn()
    {
        SetButtonActivated(true);
    }

    private void TurnOff()
    {
        SetButtonActivated(false);
    }
}

