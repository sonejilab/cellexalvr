using UnityEngine;
/// <summary>
/// Represents the button that opens the color by index menu.
/// </summary>
public class IndexMenuButton : StationaryButton
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
        CellExAlEvents.GraphsLoaded.AddListener(TurnOn);
        CellExAlEvents.GraphsUnloaded.AddListener(TurnOff);
    }

    void Start()
    {
        indexMenu = referenceManager.indexMenu.gameObject;
        buttons = referenceManager.leftButtons;
        SetButtonActivated(false);
    }

    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            spriteRenderer.sprite = standardTexture;
            descriptionText.text = "";
            controllerInside = false;
            indexMenu.SetActive(true);
            buttons.SetActive(false);
        }
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

