using UnityEngine;

/// <summary>
/// Represents a buttont that toggle all the combined arcs between all networks ina  graph skeleton,
/// The combined arcs show the number of arcs that are between two networks.
/// </summary>
public class ToggleAllCombinedArcsButton : CellexalButton
{

    public bool toggleToState;

    private NetworkCenter[] networks;

    protected override string Description
    {
        get { return "Toggle all combined arcs"; }
    }

    protected override void Click()
    {
        if (networks == null) return;

        foreach (NetworkCenter network in networks)
        {
            network.SetArcsVisible(false);
            network.SetCombinedArcsVisible(toggleToState);
        }
    }

    /// <summary>
    /// Sets the networks that this button should deal with.
    /// </summary>
    /// <param name="networks"></param>
    public void SetNetworks(NetworkCenter[] networks)
    {
        //color = network.GetComponent<Renderer>().material.color;
        //GetComponent<Renderer>().material.color = color;
        this.networks = new NetworkCenter[networks.Length];
        networks.CopyTo(this.networks, 0);

    }

    /// <summary>
    /// Show or hides all the combined arcs.
    /// </summary>
    /// <param name="visible"> True if the arcs should be shown, false if hidden. </param>
    public void SetCombinedArcsVisible(bool visible)
    {
        foreach (NetworkCenter network in networks)
        {
            network.SetCombinedArcsVisible(visible);
        }
    }
}
