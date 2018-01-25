using UnityEngine;

public class ColorByGeneMenuButton : StationaryButton
{

    private GameObject buttons;
    private GameObject colorByGeneMenu;

    protected override string Description
    {
        get
        {
            return "Show the toggle arcs menu";
        }
    }

    void Start()
    {
        buttons = referenceManager.leftButtons;
        colorByGeneMenu = referenceManager.colorByGeneMenu.gameObject;
    }

    void Update()
    {
        if (!buttonActivated) return;
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            spriteRenderer.sprite = standardTexture;
            controllerInside = false;
            colorByGeneMenu.SetActive(true);
            foreach (StationaryButton b in buttons.GetComponentsInChildren<StationaryButton>())
            {
                b.SetButtonActivated(false);
            }
        }
    }
}

