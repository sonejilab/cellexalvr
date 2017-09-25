using System;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

/// <summary>
/// This class represents a collection of networks placed on a skeleton like model of a graph.
/// </summary>
public class NetworkHandler : MonoBehaviour
{

    public Material highlightMaterial;
    public List<NetworkCenter> Replacements { get; private set; }
    public string NetworkHandlerName { get; internal set; }

    private List<NetworkCenter> networks = new List<NetworkCenter>();
    private ReferenceManager referenceManager;
    private GameManager gameManager;
    private MeshRenderer meshRenderer;
    private Material[] highlightedMaterials;
    private Material[] unhighlightedMaterials;

    private void Start()
    {
        Replacements = new List<NetworkCenter>();
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        gameManager = referenceManager.gameManager;
        meshRenderer = GetComponent<MeshRenderer>();
        highlightedMaterials = new Material[] { meshRenderer.materials[0], highlightMaterial };
        unhighlightedMaterials = new Material[] { meshRenderer.materials[0], null };
    }
    private void Update()
    {
        if (GetComponent<VRTK_InteractableObject>().enabled)
        {
            gameManager.InformMoveNetwork(NetworkHandlerName, transform.position, transform.rotation, transform.localScale);
        }
    }

    /// <summary>
    /// Adds a networkcenter to this handler.
    /// </summary>
    /// <param name="network"> The networkcenter to add. </param>
    internal void AddNetwork(NetworkCenter network)
    {
        networks.Add(network);
    }

    /// <summary>
    /// Highlights this networkhandler by outlining the skeleton mesh.
    /// </summary>
    public void Highlight()
    {
        meshRenderer.materials = highlightedMaterials;
    }

    /// <summary>
    /// Unhighlights this networkhandler.
    /// </summary>
    public void Unhighlight()
    {
        meshRenderer.materials = unhighlightedMaterials;
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

    /// <summary>
    /// Finds a networkcenter in this networkhandler.
    /// </summary>
    /// <param name="networkCenterName"> The name of the networkcenter </param>
    /// <returns> A reference to the networkcenter, or null if it was not found. </returns>
    public NetworkCenter FindNetworkCenter(string networkCenterName)
    {
        foreach (NetworkCenter network in networks)
        {
            if (network.NetworkCenterName == networkCenterName)
            {
                return network;
            }
        }
        return null;
    }
}

