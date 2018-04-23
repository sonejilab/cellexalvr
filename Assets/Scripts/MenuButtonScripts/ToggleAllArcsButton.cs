using UnityEngine;

/// <summary>
/// Represents a button that toggle all arcs between all networks on a graph skeleton.
/// </summary>
public class ToggleAllArcsButton : CellexalButton
{
    public bool toggleToState;

    private NetworkCenter[] networks;

    protected override string Description
    {
        get { return "Toggle all arcs"; }
    }

    protected override void Click()
    {
        if (networks == null) return;
        foreach (NetworkCenter network in networks)
        {
            network.SetCombinedArcsVisible(false);
            network.SetArcsVisible(toggleToState);
        }
    }

    /// <summary>
    /// Sets the networks that this button should deal with.
    /// </summary>
    public void SetNetworks(NetworkCenter[] networks)
    {
        //color = network.GetComponent<Renderer>().material.color;
        //GetComponent<Renderer>().material.color = color;
        this.networks = new NetworkCenter[networks.Length];
        networks.CopyTo(this.networks, 0);
    }

}
