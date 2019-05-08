using CellexalVR.AnalysisLogic;
using CellexalVR.General;
using CellexalVR.Interaction;
using System.Collections.Generic;
using UnityEngine;
using VRTK;

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
        //public string NetworkHandlerName { get; internal set; }

        public List<NetworkCenter> networks = new List<NetworkCenter>();

        private ReferenceManager referenceManager;
        private GameManager gameManager;
        private MeshRenderer meshRenderer;
        private Material[] highlightedMaterials;
        private Material[] unhighlightedMaterials;

        // For minimization animation
        private bool minimize;
        private bool maximize;
        private float speed;
        private float targetMinScale;
        private float targetMaxScale;
        private float shrinkSpeed;
        private Vector3 originalPos;
        private Quaternion originalRot;
        private Vector3 originalScale;

        private bool createAnim;
        private Vector3 targetPos;
        private float targetScale;

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
            speed = 1.5f;
            shrinkSpeed = 2f;
            targetMinScale = 0.05f;
            targetMaxScale = targetScale = 1f;
            targetPos = originalPos = originalScale = new Vector3();
            originalRot = new Quaternion();
            Replacements = new List<NetworkCenter>();
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            GetComponent<NetworkHandlerInteract>().referenceManager = referenceManager;
            gameManager = referenceManager.gameManager;
            meshRenderer = GetComponent<MeshRenderer>();
            highlightedMaterials = new Material[] { meshRenderer.materials[0], new Material(highlightMaterial) };
            highlightedMaterials[1].SetFloat("_Thickness", 0.2f);
            unhighlightedMaterials = new Material[] { meshRenderer.materials[0], new Material(normalMaterial) };
            this.transform.localScale = Vector3.zero;
            CellexalEvents.ScriptFinished.AddListener(SetRemovable);
            CellexalEvents.ScriptRunning.AddListener(SetUnRemovable);
            referenceManager.graphManager.AddNetwork(this);
        }

        private void SetUnRemovable()
        {
            removable = true;
        }

        private void SetRemovable()
        {
            removable = false;
        }

        private void Update()
        {
            if (GetComponent<VRTK_InteractableObject>().IsGrabbed())
            {
                gameManager.InformMoveNetwork(name, transform.position, transform.rotation, transform.localScale);
            }
            if (minimize)
            {
                Minimize();
            }
            if (maximize)
            {
                Maximize();
            }
            if (createAnim)
            {
                NetworkAnimation();
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
            networks.RemoveAll(item => item == null);
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
            transform.position = referenceManager.minimizedObjectHandler.transform.position;
            foreach (NetworkCenter network in Replacements)
            {
                network.GetComponent<Renderer>().enabled = true;
                network.HideSphereIfEnlarged();
            }
            foreach (NetworkCenter network in networks)
            {
                foreach (Renderer r in network.GetComponentsInChildren<Renderer>())
                    r.enabled = true;
                if (network.Enlarged)
                {
                    network.gameObject.GetComponent<Renderer>().enabled = false;
                }

            }
            GetComponent<Renderer>().enabled = true;
            GetComponent<Collider>().enabled = true;
            maximize = true;
        }

        /// <summary>
        /// Animation for showing network.
        /// </summary>
        void Maximize()
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, originalPos, step);
            transform.localScale += Vector3.one * Time.deltaTime * shrinkSpeed;
            transform.Rotate(Vector3.one * Time.deltaTime * -100);
            if (transform.localScale.x >= originalScale.x)
            {
                transform.localScale = originalScale;
                transform.localPosition = originalPos;
                foreach (NetworkCenter network in Replacements)
                {
                    network.GetComponent<Collider>().enabled = true;
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
                maximize = false;
            }
        }

        /// <summary>
        /// Toggles all renderers and colliders off for all networks on this convex hull.
        /// </summary>
        internal void HideNetworks()
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
            minimize = true;
        }

        /// <summary>
        /// Animation for hiding network.
        /// </summary>
        void Minimize()
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, referenceManager.minimizedObjectHandler.transform.position, step);
            transform.localScale -= Vector3.one * Time.deltaTime * shrinkSpeed;
            transform.Rotate(Vector3.one * Time.deltaTime * 100);
            if (transform.localScale.x <= targetMinScale)
            {
                foreach (NetworkCenter network in networks)
                {
                    foreach (Renderer r in network.GetComponentsInChildren<Renderer>())
                        r.enabled = false;
                }
                foreach (Renderer r in GetComponentsInChildren<Renderer>())
                    r.enabled = false;
                minimize = false;
                referenceManager.minimizeTool.GetComponent<Light>().range = 0.04f;
                referenceManager.minimizeTool.GetComponent<Light>().intensity = 0.8f;
            }
        }

        /// <summary>
        /// Spawn network beside graph it was created from.
        /// </summary>
        public void CreateNetworkAnimation(Transform graph)
        {
            if (gameManager.multiplayer)
            {
                transform.position = new Vector3(0, 1, 0);
            }
            if (!gameManager.multiplayer)
            {
                transform.position = graph.position;
                transform.rotation = graph.rotation;
                transform.position += transform.forward * 0.3f;
                //transform.position = referenceManager.headset.transform.position;
                //transform.rotation = referenceManager.headset.transform.rotation;
                //transform.position += transform.forward * 1f;
            }
            //transform.Rotate(-20, 0, 0);
            targetPos = transform.position;
            createAnim = true;
        }

        void NetworkAnimation()
        {
            float step = speed * Time.deltaTime;
            //transform.position = Vector3.MoveTowards(transform.position, targetPos, step);
            transform.localScale += Vector3.one * Time.deltaTime * shrinkSpeed;
            if (transform.localScale.x >= targetScale)
            {
                createAnim = false;
                referenceManager.notificationManager.SpawnNotification("Transcription factor networks finished.");

            }
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

        public void HighLightGene(string geneName)
        {
            foreach (NetworkCenter nc in networks)
            {
                nc.HighLightGene(geneName);
            }
        }
    }

}