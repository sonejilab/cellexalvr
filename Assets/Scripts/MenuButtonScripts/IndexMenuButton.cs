using UnityEngine;

public class IndexMenuButton : StationaryButton
{
    private GameObject indexMenu;
    private GameObject buttons;

    protected override string Description
    {
        get { return "Show menu for coloring by index"; }
    }


    void Start()
    {
        indexMenu = referenceManager.indexMenu.gameObject;
        buttons = referenceManager.leftButtons;
        SetButtonActivated(false);
        ButtonEvents.GraphsLoaded.AddListener(TurnOn);
        ButtonEvents.GraphsUnloaded.AddListener(TurnOff);
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

