using CellexalVR.AnalysisLogic;
using CellexalVR.General;
using CellexalVR.Interaction;
using CellexalVR.Multiuser;
using CellexalVR.Tools;
using System.Collections.Generic;
using System.Linq;
using CellexalVR.Extensions;
using CellexalVR.Menu.Buttons.Networks;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using DG.Tweening;

namespace CellexalVR.AnalysisObjects
{
    /// <summary>
    /// Represents a collection of networks placed on a skeleton like model of a graph.
    /// </summary>
    public class NetworkHandler : MonoBehaviour
    {
        public Material highlightMaterial;
        public Material normalMaterial;
        public List<NetworkCenter> Replacements { get; private set; }
        public bool removable;

        public bool removing = false;
        // public GameObject wirePrefab;

        public ReferenceManager referenceManager;
        //public string NetworkHandlerName { get; internal set; }

        public List<NetworkCenter> networks = new List<NetworkCenter>();

        private MultiuserMessageSender multiuserMessageSender;
        private MeshRenderer meshRenderer;
        private Material[] highlightedMaterials;
        private Material[] unhighlightedMaterials;

        // For minimization animation
        private Vector3 originalPos;
        private Quaternion originalRot;
        private Vector3 originalScale;

        private float animationTime = 0.8f;

        private GameObject previewWire;
        private bool buttonClickedThisFrame;
        private ToggleArcsButton previouslyClickedButton;
        private readonly Color highlightColor = Definitions.TronColor;
        private Color standardColor;

        public int layoutApplied = 0;

        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        private void Start()
        {
            originalPos = originalScale = new Vector3();
            originalRot = new Quaternion();
            Replacements = new List<NetworkCenter>();
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            GetComponent<NetworkHandlerInteract>().referenceManager = referenceManager;
            multiuserMessageSender = referenceManager.multiuserMessageSender;
            meshRenderer = GetComponent<MeshRenderer>();
            standardColor = GetComponent<LineRenderer>().startColor;
            // highlightedMaterials = new Material[] {meshRenderer.materials[0], new Material(highlightMaterial)};
            // highlightedMaterials[1].SetFloat("_Thickness", 0.2f);
            // unhighlightedMaterials = new Material[] {meshRenderer.materials[0], new Material(normalMaterial)};
            this.transform.localScale = Vector3.zero;
            CellexalEvents.ScriptFinished.AddListener(() => removable = true);
            CellexalEvents.ScriptRunning.AddListener(() => removable = false);
            referenceManager.graphManager.AddNetwork(this);
            // wirePrefab = referenceManager.filterManager.wirePrefab;
            // previewWire = Instantiate(referenceManager.arcsSubMenu.wirePrefab, this.transform);
            // previewWire.SetActive(false);
        }


        private void Update()
        {
            if (GetComponent<XRGrabInteractable>().isSelected)
            {
                multiuserMessageSender.SendMessageMoveNetwork(name, transform.position, transform.rotation,
                    transform.localScale);
            }
        }

        /// <summary>
        /// Calculates the 2D layout of all networks.
        /// </summary>
        public void CalculateLayoutOnAllNetworks()
        {
            foreach (var network in networks)
            {
                network.CalculateLayout(NetworkCenter.Layout.TWO_D);
            }
        }

        /// <summary>
        /// Saves the arcs that go between the networks.
        /// </summary>
        /// <param name="keyPairs">An array of <see cref="NetworkReader.NetworkKeyPair"/>.</param>
        /// <param name="nodes">All nodes in all networks.</param>
        public void CreateArcs(ref NetworkReader.NetworkKeyPair[] keyPairs, ref Dictionary<string, NetworkNode> nodes)
        {
            // since the list is sorted in a smart way, all keypairs that share a key will be next to eachother
            var lastKey = new NetworkReader.NetworkKeyPair("", "", "", "", "");
            List<NetworkReader.NetworkKeyPair> lastNodes = new List<NetworkReader.NetworkKeyPair>();
            for (int i = 0; i < keyPairs.Length; ++i)
            {
                NetworkReader.NetworkKeyPair keypair = keyPairs[i];
                // if this keypair shares a key with the last keypair
                if (lastKey.key1 == keypair.key1 || lastKey.key1 == keypair.key2)
                {
                    // add arcs to all previous pairs that also shared a key
                    foreach (NetworkReader.NetworkKeyPair node in lastNodes)
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
                // Also add toggle arcs buttons. 
                GameObject newButton = Instantiate(referenceManager.arcsSubMenu.buttonPrefab, transform);
                newButton.layer = LayerMask.NameToLayer("EnvironmentButtonLayer");
                ToggleArcsButton toggleArcButton = newButton.GetComponent<ToggleArcsButton>();
                referenceManager.arcsSubMenu.toggleArcButtonList.Add(toggleArcButton);
                newButton.transform.localScale = network.transform.localScale / 6;
                newButton.transform.localPosition = network.transform.localPosition + new Vector3(0, 0.13f, 0);
                newButton.name = network.gameObject.name + "_ArcButton";
                newButton.gameObject.SetActive(true);
                newButton.GetComponent<Renderer>().enabled = true;
                newButton.GetComponent<Collider>().enabled = true;
                Color color = network.GetComponent<Renderer>().material.color;
                toggleArcButton.ButtonColor = color;
                toggleArcButton.SetNetwork(network);
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
            networks.RemoveAll(item => item == null);
        }

        /// <summary>
        /// Toggles all renderers and colliders on for all networks on this convex hull.
        /// </summary>
        internal void ShowNetworks()
        {
            transform.position = referenceManager.leftController.transform.position;
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                r.enabled = true;
            foreach (NetworkCenter network in networks)
            {
                network.HideSphereIfEnlarged();
                foreach (Renderer r in network.GetComponentsInChildren<Renderer>())
                    r.enabled = true;

                if (network.Enlarged)
                {
                    network.gameObject.GetComponent<Renderer>().enabled = false;
                }
            }

            transform.DOLocalMove(originalPos, animationTime).SetEase(Ease.OutCubic);
            transform.DOLocalRotate(originalRot.eulerAngles, animationTime, RotateMode.FastBeyond360).SetEase(Ease.OutCubic);
            transform.DOScale(Vector3.one, animationTime).SetEase(Ease.InCubic).OnComplete(() => OnShowComplete());
            GetComponent<Renderer>().enabled = true;
        }

        private void OnShowComplete()
        {
            foreach (NetworkCenter network in Replacements)
            {
                network.GetComponent<Collider>().enabled = true;
            }
            foreach (ToggleArcsButton button in GetComponentsInChildren<ToggleArcsButton>())
            {
                button.GetComponent<Collider>().enabled = true;
            }
            foreach (NetworkCenter network in networks)
            {
                network.GetComponent<Collider>().enabled = true;
                if (network.Enlarged)
                {
                    foreach (Collider c in network.GetComponentsInChildren<Collider>())
                        c.enabled = true;
                }
            }
            GetComponent<Collider>().enabled = true;
        }

        /// <summary>
        /// Toggles all renderers and colliders off for all networks on this convex hull.
        /// </summary>
        internal void HideNetworks(bool delete = false)
        {
            foreach (NetworkCenter network in networks)
            {
                foreach (Collider c in network.GetComponentsInChildren<Collider>())
                    c.enabled = false;
            }

            foreach (Collider c in GetComponentsInChildren<Collider>())
                c.enabled = false;
            originalPos = transform.position;
            originalRot = transform.localRotation;
            originalScale = transform.localScale;

            Vector3 targetPosition;
            if (delete)
            {
                targetPosition = referenceManager.deleteTool.transform.position;
            }
            else if (referenceManager.menuToggler.MenuActive)
            {
                targetPosition = referenceManager.minimizedObjectHandler.transform.position;
            }
            else
            {
                targetPosition = referenceManager.menuToggler.menuCube.transform.position;
            }
            transform.DOLocalMove(targetPosition, animationTime).SetEase(Ease.InCubic);
            transform.DOLocalRotate(new Vector3(0, 360, 0), animationTime, RotateMode.FastBeyond360).SetEase(Ease.InCubic);
            transform.DOScale(Vector3.zero, animationTime).SetEase(Ease.OutCubic).OnComplete(() => OnHideComplete(delete));
        }

        private void OnHideComplete(bool delete = false)
        {
            if (delete)
            {
                referenceManager.deleteTool.GetComponent<RemovalController>().ResetHighlight();
                Destroy(gameObject);
                return;
            }
            foreach (NetworkCenter network in networks)
            {
                foreach (Renderer r in network.GetComponentsInChildren<Renderer>())
                    r.enabled = false;
            }
            foreach (Renderer r in GetComponentsInChildren<Renderer>())
                r.enabled = false;
            referenceManager.minimizeTool.GetComponent<Light>().range = 0.04f;
            referenceManager.minimizeTool.GetComponent<Light>().intensity = 0.8f;
        }


        /// <summary>
        /// Spawn network beside graph it was created from.
        /// </summary>
        public void CreateNetworkAnimation(Transform graph)
        {
            if (multiuserMessageSender.multiplayer)
            {
                transform.position = new Vector3(0, 1, 0);
            }

            if (!multiuserMessageSender.multiplayer)
            {
                transform.position = graph.position;
                transform.rotation = graph.rotation;
                transform.position += transform.forward * 0.3f;
            }

            transform.DOScale(Vector3.one, animationTime + 0.3f).SetEase(Ease.OutBounce).OnComplete(() => OnShowComplete());

        }

        /// <summary
        /// <summary>
        /// Used to delete this network. Starts the same animation that is used to minimze but deletes the obj when its been minimized.
        /// </summary>
        public void DeleteNetwork()
        {
            if (!removable)
            {
                Debug.Log("Script is running");
                CellexalError.SpawnError("Delete failed",
                    "Can not delete network yet. Wait for script to finish before removing it.");
            }

            for (int i = 0; i < networks.Count; i++)
            {
                Destroy(networks[i].gameObject);
            }

            networks.Clear();
            referenceManager.arcsSubMenu.DestroyTab(name.Split('_')[1]); // Get last part of nw name   
            foreach (GameObject wire in referenceManager.arcsSubMenu.toggleArcButtonList.SelectMany(toggleArcsButton => toggleArcsButton.wires))
            {
                Destroy(wire);
            }

            referenceManager.networkGenerator.networkList.RemoveAll(item => item == null);
            referenceManager.graphManager.RemoveNetwork(this);
            HideNetworks(true);
            removing = true;
            //Destroy(this.gameObject);
            //referenceManager.deleteTool.GetComponent<RemovalController>().DeleteObjectAnimation(this.gameObject);
        }

        /// <summary>
        /// Toggles all colliders in all networks. Used the networks are being grabbed to reduce lag.
        /// </summary>
        /// <param name="newState">True to toggle the colliders on, false otherwise.</param>
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

        /// <summary>
        /// Highlights a gene in all networks with a red circle.
        /// </summary>
        /// <param name="geneName">The gene to highlight.</param>
        public void HighLightGene(string geneName)
        {
            foreach (NetworkCenter nc in networks)
            {
                nc.HighLightGene(geneName);
                referenceManager.multiuserMessageSender.SendMessageHighlightNetworkNode(name, nc.name, geneName);
            }
        }

        public void HighlightNetworkSkeleton(bool toggle)
        {
            LineRenderer lr = GetComponent<LineRenderer>();
            lr.startColor = lr.endColor = toggle ? highlightColor : standardColor;
        }
    }
}