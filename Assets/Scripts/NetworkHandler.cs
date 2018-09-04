using System;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

/// <summary>
/// Represents a collection of networks placed on a skeleton like model of a graph.
/// </summary>
public class NetworkHandler : MonoBehaviour
{

    public Material highlightMaterial;
    public List<NetworkCenter> Replacements { get; private set; }
    //public string NetworkHandlerName { get; internal set; }

    private List<NetworkCenter> networks = new List<NetworkCenter>();
    private ReferenceManager referenceManager;
    private GameManager gameManager;
    private MeshRenderer meshRenderer;
    private Material[] highlightedMaterials;
    private Material[] unhighlightedMaterials;

    public int layoutApplied = 0;

    private void Start()
    {
        Replacements = new List<NetworkCenter>();
        referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        GetComponent<NetworkHandlerInteract>().referenceManager = referenceManager;
        gameManager = referenceManager.gameManager;
        meshRenderer = GetComponent<MeshRenderer>();
        highlightedMaterials = new Material[] { meshRenderer.materials[0], new Material(highlightMaterial) };
        highlightedMaterials[1].SetFloat("_Thickness", 0.2f);
        unhighlightedMaterials = new Material[] { meshRenderer.materials[0], null };
    }

    private void Update()
    {
        if (GetComponent<VRTK_InteractableObject>().enabled)
        {
            gameManager.InformMoveNetwork(name, transform.position, transform.rotation, transform.localScale);
        }
    }

    public void CalculateLayoutOnAllNetworks()
    {
        foreach (var network in networks)
        {
            network.CalculateLayout(NetworkCenter.Layout.TWO_D);
        }
    }

    public void CreateArcs(ref InputReader.NetworkKeyPair[] keyPairs, ref Dictionary<string, NetworkNode> nodes)
    {
        // since the list is sorted in a smart way, all keypairs that share a key will be next to eachother
        var lastKey = new InputReader.NetworkKeyPair("", "", "", "", "");
        List<InputReader.NetworkKeyPair> lastNodes = new List<InputReader.NetworkKeyPair>();
        for (int i = 0; i < keyPairs.Length; ++i)
        {
            InputReader.NetworkKeyPair keypair = keyPairs[i];
            // if this keypair shares a key with the last keypair
            if (lastKey.key1 == keypair.key1 || lastKey.key1 == keypair.key2)
            {
                // add arcs to all previous pairs that also shared a key
                foreach (InputReader.NetworkKeyPair node in lastNodes)
                {
                    var center = nodes[node.node1].Center;
                    center.AddArc(nodes[node.node1], nodes[node.node2], nodes[keypair.node1], nodes[keypair.node2]);
                }
            }
            else
            {
                // clear the list if this key did not match the last one
                lastNodes.Clear();
            }
            lastNodes.Add(keypair);
            lastKey = keypair;
        }


        // copy the networks to an array
        NetworkCenter[] networkCenterArray = new NetworkCenter[networks.Count];
        int j = 0;
        foreach (NetworkCenter n in networks)
        {
            networkCenterArray[j++] = n;
        }

        // create the toggle arcs menu and its buttons
        referenceManager.arcsSubMenu.CreateToggleArcsButtons(networkCenterArray);

        List<int> arcsCombinedList = new List<int>();
        foreach (NetworkCenter network in networks)
        {
            var arcscombined = network.CreateCombinedArcs();
            arcsCombinedList.Add(arcscombined);
            // toggle the arcs off
            network.SetArcsVisible(false);
            network.SetCombinedArcsVisible(false);
        }

        // figure out how many combined arcs there are
        var max = 0;
        foreach (int i in arcsCombinedList)
        {
            if (max < i)
                max = i;
        }

        // color all combined arcs
        foreach (NetworkCenter network in networks)
        {
            network.ColorCombinedArcs(max);
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
            network.GetComponent<Renderer>().enabled = true;
            network.GetComponent<Collider>().enabled = true;
            network.HideSphereIfEnlarged();
        }
        foreach (NetworkCenter network in networks)
        {
            foreach (Renderer r in network.GetComponentsInChildren<Renderer>())
                r.enabled = true;
            network.GetComponent<Collider>().enabled = true;
            if (network.Enlarged)
            {
                network.gameObject.GetComponent<Renderer>().enabled = false;
                foreach (Collider c in network.GetComponentsInChildren<Collider>())
                    c.enabled = true;
            }

        }
        GetComponent<Renderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
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

    internal void ToggleNetworkColliders(bool newState)
    {
        foreach (NetworkCenter network in networks)
        {
            if (network.Enlarged)
            {
                foreach (Collider c in network.GetComponentsInChildren<Collider>())
                {
                    c.enabled = newState;
                }
            }
        }
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
            if (network.name == networkCenterName)
            {
                return network;
            }
        }
        return null;
    }
}

