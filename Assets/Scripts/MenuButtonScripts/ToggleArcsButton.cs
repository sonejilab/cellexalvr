using UnityEngine;

/// <summary>
/// Represents a button that toggles arcs between networks.
/// </summary>
public class ToggleArcsButton : CellexalButton
{
    public bool toggleToState;

    [HideInInspector]
    public ToggleAllCombinedArcsButton combinedNetworksButton;
    private NetworkCenter network;

    protected override string Description
    {
        get { return "Toggle all arcs connected to this network"; }
    }

    protected override void Click()
    {
        combinedNetworksButton.SetCombinedArcsVisible(false);
        network.SetArcsVisible(toggleToState);
    }

    /// <summary>
    /// Sets which network's arcs this button should toggle.
    /// </summary>
    public void SetNetwork(NetworkCenter network)
    {
        this.network = network;
    }
}
