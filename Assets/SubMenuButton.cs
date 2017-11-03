using UnityEngine;

class SubMenuButton : StationaryButton
{
    public string description;
    public GameObject buttonsToDeactivate;
    public GameObject menu;

    protected override string Description
    {
        get { return description; }
    }


    private void Start()
    {
        // The gameobject should be active but the renderers and colliders should be disabled.
        // This makes the buttons in the menu able to receive events while not being shown.
        menu.SetActive(true);
        SetMenuActivated(false);
    }


    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            foreach (StationaryButton b in buttonsToDeactivate.GetComponentsInChildren<StationaryButton>())
            {
                b.SetButtonActivated(false);
            }
            SetMenuActivated(true);
        }
    }

    private void SetMenuActivated(bool activate)
    {
        foreach (Renderer r in menu.GetComponentsInChildren<Renderer>())
            r.enabled = activate;
        foreach (Collider c in menu.GetComponentsInChildren<Collider>())
            c.enabled = activate;
    }
}
