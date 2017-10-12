using UnityEngine;

/// <summary>
/// This class represents a button that toggles arcs between networks.
/// </summary>
public class ToggleArcsButton : SolidButton
{
    public bool toggleToState;

    [HideInInspector]
    public ToggleAllCombinedArcsButton combinedNetworksButton;
    private NetworkCenter network;

    void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            combinedNetworksButton.SetCombinedArcsVisible(false);
            network.SetArcsVisible(toggleToState);
        }
    }

    /// <summary>
    /// Sets which network's arcs this button should toggle.
    /// </summary>
    public void SetNetwork(NetworkCenter network)
    {
        this.network = network;
    }
}
