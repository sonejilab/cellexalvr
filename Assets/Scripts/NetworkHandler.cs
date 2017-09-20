using System;
using System.Collections.Generic;
using UnityEngine;
using VRTK;


public class NetworkHandler : MonoBehaviour
{

    private List<NetworkCenter> networks = new List<NetworkCenter>();
    public List<NetworkCenter> Replacements { get; private set; }
    public string NetworkHandlerName { get; internal set; }
    private ReferenceManager referenceManager;
    private GameManager gameManager;

    private void Start()
    {
        Replacements = new List<NetworkCenter>();
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        gameManager = referenceManager.gameManager;
    }
    private void Update()
    {
        if (GetComponent<VRTK_InteractableObject>().enabled)
        {
            gameManager.InformMoveNetwork(NetworkHandlerName, transform.position, transform.rotation);
        }
    }


    internal void AddNetwork(NetworkCenter network)
    {
        networks.Add(network);
    }


    /// <summary>
    /// Toggles all renderers and colliders on for all networks on this convex hull.
    /// </summary>
    internal void ShowNetworks()
    {
        foreach (NetworkCenter network in Replacements)
        {
            network.HideSphereIfEnlarged();
        }
        foreach (NetworkCenter network in networks)
        {
            foreach (Renderer r in network.GetComponentsInChildren<Renderer>())
                r.enabled = true;
            foreach (Collider c in network.GetComponentsInChildren<Collider>())
                c.enabled = true;
            if (network.Enlarged)
            {
                // turn off the renderer for the sphere
                network.gameObject.GetComponent<Renderer>().enabled = false;
            }
        }
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = true;
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = true;
    }

    /// <summary>
    /// Toggles all renderers and colliders off for all networks on this convex hull.
    /// </summary>
    internal void HideNetworks()
    {
        foreach (NetworkCenter network in networks)
        {
            foreach (Renderer r in network.GetComponentsInChildren<Renderer>())
                r.enabled = false;
            foreach (Collider c in network.GetComponentsInChildren<Collider>())
                c.enabled = false;
        }
        foreach (Renderer r in GetComponentsInChildren<Renderer>())
            r.enabled = false;
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.enabled = false;
    }

    public NetworkCenter FindNetworkCenter(string networkName)
    {
        foreach (NetworkCenter network in networks)
        {
            if (network.NetworkName == networkName)
            {
                return network;
            }
        }
        return null;
    }
}

