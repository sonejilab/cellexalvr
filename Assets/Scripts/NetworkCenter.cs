using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Color = UnityEngine.Color;
using System.Drawing;
using VRTK;
using VRTK.GrabAttachMechanics;
using VRTK.SecondaryControllerGrabActions;
using System.IO;
using System.Drawing.Imaging;

/// <summary>
/// Represents the center of a network. It handles the enlarging when it is pressed.
/// </summary>
public class NetworkCenter : MonoBehaviour
{
    public GameObject replacementPrefab;
    public GameObject edgePrefab;
    public GameObject arcDescriptionPrefab;
    public GameObject simpleArcDescriptionPrefab;
    public List<Color> combinedArcsColors;
    public NetworkHandler Handler { get; set; }


    public float MaxNegPcor { get; set; }
    public float MinNegPcor { get; set; }
    public float MaxPosPcor { get; set; }
    public float MinPosPcor { get; set; }
    private int layoutSeed;
    public int LayoutSeed
    {
        get { return layoutSeed; }
        set
        {
            layoutSeed = value;
            rand = new System.Random(value);
        }
    }

    private ControllerModelSwitcher controllerModelSwitcher;
    // The network will pop up above the pedestal gameobject when it's enlarged.
    private GameObject pedestal;
    private SteamVR_Controller.Device device;
    private bool controllerInside = false;
    private Vector3 oldLocalPosition;
    private Vector3 oldScale;
    private Quaternion oldRotation;
    private Transform oldParent;
    public bool Enlarged { get; private set; }
    private bool enlarge = false;
    private int numColliders = 0;
    public bool isReplacement = false;
    private List<NetworkNode> nodes = new List<NetworkNode>();
    [HideInInspector]
    public NetworkCenter replacing;
    private List<Arc> arcs = new List<Arc>();
    private List<CombinedArc> combinedArcs = new List<CombinedArc>();
    private SteamVR_TrackedObject rightController;
    private NetworkGenerator networkGenerator;
    private GameManager gameManager;
    public enum Layout { TWO_D, THREE_D }
    private Layout currentLayout;
    private bool[] layoutsCalculated = { false, false };
    private bool calculatingLayout = false;
    private bool switchingLayout = false;
    private Dictionary<NetworkNode, Vector3> positions;
    private System.Random rand;
    private string oldName;

    void Start()
    {
        var referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        pedestal = GameObject.Find("Pedestal");
        rightController = referenceManager.rightController;
        networkGenerator = referenceManager.networkGenerator;
        controllerModelSwitcher = referenceManager.controllerModelSwitcher;
        gameManager = referenceManager.gameManager;
    }

    void FixedUpdate()
    {
        // moving kinematic rigidbodies
        if (enlarge)
        {
            enlarge = false;
            if (!isReplacement && !Enlarged)
            {
                gameManager.InformEnlargeNetwork(Handler.name, name);
                EnlargeNetwork();
            }
            if (isReplacement)
            {
                gameManager.InformBringBackNetwork(Handler.name, replacing.name);
                BringBackOriginal();
            }
        }
    }

    void Update()
    {
        // handle input
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            controllerInside = false;
            numColliders = 0;
            enlarge = true;
        }
        if (gameObject.transform.hasChanged)
        {
            foreach (Arc a in arcs)
            {
                Vector3 midPoint1 = (a.t1.position + a.t2.position) / 2f;
                Vector3 midPoint2 = (a.t3.position + a.t4.position) / 2f;
                a.renderer.SetPositions(new Vector3[] { midPoint1, midPoint2 });
            }

            foreach (CombinedArc a in combinedArcs)
            {
                if (a.center1 != this)
                    a.renderer.SetPositions(new Vector3[] { transform.position, a.center2.position });
                else
                    a.renderer.SetPositions(new Vector3[] { transform.position, a.center1.position });
            }
        }

        var interactableObject = GetComponent<VRTK_InteractableObject>();
        if (interactableObject)
        {
            if (interactableObject.enabled)
            {
                gameManager.InformMoveNetworkCenter(Handler.name, name, transform.position, transform.rotation, transform.localScale);
            }
        }

    }

    /// <summary>
    /// Adds a node to this network.
    /// </summary>
    /// <param name="newNode">The node to add.</param>
    public void AddNode(NetworkNode newNode)
    {
        nodes.Add(newNode);
    }

    /// <summary>
    /// Calcualte a new layout.
    /// </summary>
    /// <param name="layout">The desired type of layout.</param>
    public void CalculateLayout(Layout layout)
    {
        if (calculatingLayout)
            return;
        currentLayout = layout;
        StartCoroutine(CalculateLayoutCoroutine(layout));

    }
    /// <summary>
    /// Starts a thread that calculates the layout.
    /// </summary>
    /// <param name="layout">The desired layout.</param>
    private IEnumerator CalculateLayoutCoroutine(Layout layout)
    {
        //var c = GetComponent<Renderer>().material.color;
        //GetComponent<Renderer>().material.color = new Color(c.r, c.g, c.b, 0f);
        Dictionary<NetworkNode, Vector3> positions = new Dictionary<NetworkNode, Vector3>(nodes.Count);
        foreach (var node in nodes)
        {
            positions[node] = node.transform.localPosition;
        }

        Thread t = new Thread(() => CalculateLayoutThread(layout, positions));
        t.Start();
        while (t.IsAlive)
            yield return null;
        t.Join();
        foreach (var nodePos in positions)
        {
            nodePos.Key.transform.localPosition = nodePos.Value;
            nodePos.Key.RepositionEdges();
        }
        positions.Clear();
    }

    /// <summary>
    /// Method that is run as a thread to calculate layout
    /// </summary>
    /// <param name="layout">The desired layout.</param>
    /// <param name="positions">The variable to write the positions to.</param>
    private void CalculateLayoutThread(Layout layout, Dictionary<NetworkNode, Vector3> positions)
    {
        calculatingLayout = true;
        float desiredSpringLength = 0.07f;
        int iterations = 100;
        float springConstant = 0.15f;
        float nonAdjecentNeighborConstant = 0.0003f;



        // start by giving all vertices a random position
        if (layout == Layout.THREE_D)
        {
            foreach (var node in nodes)
            {
                positions[node] = new Vector3((float)rand.NextDouble() - 0.5f, (float)rand.NextDouble() - 0.5f, (float)rand.NextDouble() - 0.5f);
            }
        }
        else if (layout == Layout.TWO_D)
        {
            foreach (var node in nodes)
            {
                positions[node] = new Vector3((float)rand.NextDouble() - 0.5f, (float)rand.NextDouble() - 0.5f, 0f);
            }
        }

        Dictionary<NetworkNode, Vector3> forces = new Dictionary<NetworkNode, Vector3>(nodes.Count);

        Dictionary<int, int> swaps = new Dictionary<int, int>(nodes.Count);
        HashSet<NetworkNode> nodeSet = new HashSet<NetworkNode>(nodes);
        int nGroups = 0;
        while (nodeSet.Count > 0)
        {
            var group = nodeSet.First().AllConnectedNodes();
            foreach (var node in group)
            {
                nodeSet.Remove(node);
            }
            nGroups++;
        }
        //print(NetworkCenterName + " " + nGroups);
        if (nGroups < 9)
        {
            desiredSpringLength *= (1f / Mathf.Log(nGroups + 1, 9f));
            //nonAdjecentNeighborConstant *= (1f / Mathf.Log(nGroups + 1, 10f));
        }
        List<NetworkNode> removedNodes = new List<NetworkNode>();
        for (int i = 0; i < iterations; ++i)
        {
            float totalDistanceMoved = 0f;

            // set all forces on all vertices to zero
            foreach (var node in nodes)
            {
                forces[node] = Vector3.zero;
            }

            foreach (var node1 in nodes)
            {
                nodeSet.Remove(node1);
                removedNodes.Add(node1);
                // move all neighbours according to their springs
                foreach (var neighbour in node1.neighbours)
                {
                    nodeSet.Remove(neighbour);
                    removedNodes.Add(neighbour);

                    var diff = (positions[neighbour] - positions[node1]);
                    var dir = diff.normalized;

                    var appliedForce = diff * Mathf.Log(diff.magnitude / (desiredSpringLength * Mathf.Log(node1.neighbours.Count + 3f, 4f))) / node1.neighbours.Count;
                    //if (appliedForce.magnitude < minimumForce)
                    //    continue;
                    forces[node1] += appliedForce * springConstant;
                }

                // move all nonadjecent nodes away from eachother
                foreach (var node2 in nodeSet)
                {
                    var distance = Vector3.Distance(positions[node1], positions[node2]);
                    if (distance > 0.1f)
                        continue;
                    if (distance < 0.001f)
                    {
                        distance = 0.001f;
                    }
                    var dir = (positions[node2] - positions[node1]);
                    var appliedForce = dir.normalized / (distance * distance * nodes.Count);
                    //if (appliedForce.magnitude > maximumForce)
                    //    appliedForce = appliedForce.normalized * maximumForce;
                    //  if (appliedForce.magnitude < minimumForce)
                    //    continue;
                    forces[node1] -= appliedForce * nonAdjecentNeighborConstant;
                }
                foreach (var removedNode in removedNodes)
                {
                    nodeSet.Add(removedNode);
                }
                removedNodes.Clear();
            }

            //yield return null;

            if (layout == Layout.TWO_D)
            {
                // swap positions of vertices that have edges that cross eachother
                // only do this every fifth iteration, and only after atleast 20 iterations
                if (i % 5 == 0 && i > 20)
                {
                    foreach (var node1 in nodes)
                    {
                        foreach (var node2 in nodes)
                        {
                            if (node1 == node2)
                                continue;

                            foreach (var neighbour1 in node1.neighbours)
                            {
                                if (neighbour1 == node2)
                                    continue;

                                foreach (var neighbour2 in node2.neighbours)
                                {
                                    if (node1 == neighbour2 || neighbour1 == neighbour2)
                                        continue;
                                    // only swap nodes if they have not been swapped 3 times already (to avoid swapping nodes back and forth too many times)
                                    int hash = CombinedHashCode(node1, node2, neighbour1, neighbour2);
                                    if (!swaps.ContainsKey(hash))
                                    {
                                        swaps[hash] = 0;
                                    }
                                    else if (swaps[hash] >= 3)
                                    {
                                        continue;
                                    }

                                    var node1pos = positions[node1];
                                    var neighbour1pos = positions[neighbour1];
                                    var node2pos = positions[node2];
                                    var neighbour2pos = positions[neighbour2];

                                    float bottom = (node1pos.x - neighbour1pos.x) * (node2pos.y - neighbour2pos.y) - (node1pos.y - neighbour1pos.y) * (node2pos.x - neighbour2pos.x);
                                    if (bottom == 0f)
                                    {
                                        // avoid division by zero
                                        bottom = 0.00001f;
                                    }
                                    // find intersection coordinates through determinants, thanks wikipedia
                                    float top1 = (node1pos.x * neighbour1pos.y - node1pos.y * neighbour1pos.x);
                                    float top2 = (node2pos.x * neighbour2pos.y - node2pos.y * neighbour2pos.x);
                                    float intersectX = (top1 * (node2pos.x - neighbour2pos.x) - (node1pos.x - neighbour1pos.x) * top2) / bottom;
                                    float intersectY = (top1 * (node2pos.y - neighbour2pos.y) - (node1pos.y - neighbour1pos.y) * top2) / bottom;

                                    var intersect = new Vector3(intersectX, intersectY, 0f);

                                    if (Between(intersect, node1pos, neighbour1pos) && Between(intersect, node2pos, neighbour2pos))
                                    {
                                        swaps[hash]++;
                                        positions[neighbour1] = neighbour2pos;
                                        positions[neighbour2] = neighbour1pos;
                                        totalDistanceMoved += (neighbour1pos - neighbour2pos).magnitude;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // move all vertices according to the force affecting them
            foreach (var force in forces)
            {
                var node = force.Key;
                var pos = positions[node];
                positions[node] += force.Value;
                totalDistanceMoved += force.Value.magnitude;
                // move all vertices that are outside the circle to the edge
                if (positions[node].magnitude > 0.4f)
                {
                    positions[node] = positions[node].normalized * 0.4f;
                }
                //node.transform.localPosition = positions[node];
            }

            // do
            // {
            //     yield return null;
            // } while (!Input.GetKey(KeyCode.T));

            // stop if the nodes have not moved very much
            if (totalDistanceMoved / nodes.Count < 0.0002f)
            {
                break;
            }
            /*
            if (i % iterationsPerFrame == 0)
            {
                yield return null;

                if (Time.deltaTime > 0.025f)
                {
                    if (iterationsPerFrame > 1)
                    {
                        iterationsPerFrame--;
                    }
                }
                else if (Time.deltaTime < 0.010f)
                {
                    iterationsPerFrame++;
                }
            }
            */
        }

        foreach (var node in nodes)
        {
            if (layout == Layout.TWO_D)
            {
                layoutsCalculated[0] = true;
                node.LayoutPositions[0] = positions[node];
            }
            else if (layout == Layout.THREE_D)
            {
                layoutsCalculated[1] = true;
                node.LayoutPositions[1] = positions[node];
            }
            //node.transform.localPosition = positions[node];

            //node.RepositionEdges();
            //node.gameObject.GetComponent<BoxCollider>().enabled = false;
        }
        calculatingLayout = false;
        Handler.layoutApplied++;
    }

    private struct Edge
    {
        Vector3 v1;
        Vector3 v2;

        //public int GetHash
    }

    /// <summary>
    /// Finds if a point is between the rectangle defined whose corners are in v1 and v2
    /// </summary>
    private bool Between(Vector3 p, Vector3 v1, Vector3 v2)
    {
        float smallestX = Mathf.Min(v1.x, v2.x);
        float largestX = Mathf.Max(v1.x, v2.x);
        float smallestY = Mathf.Min(v1.y, v2.y);
        float largestY = Mathf.Max(v1.y, v2.y);
        return p.x > smallestX && p.x < largestX && p.y > smallestY && p.y < largestY;
    }

    private int CombinedHashCode(NetworkNode node1, NetworkNode node2, NetworkNode node3, NetworkNode node4)
    {
        return (node1.GetHashCode() ^ node2.GetHashCode()) ^ (node3.GetHashCode() ^ node4.GetHashCode());
    }

    private class TupleComparer : IEqualityComparer<Tuple<NetworkNode, NetworkNode>>
    {
        public bool Equals(Tuple<NetworkNode, NetworkNode> x, Tuple<NetworkNode, NetworkNode> y)
        {
            return x.Item1 == y.Item1 && x.Item2 == y.Item2 || x.Item1 == y.Item2 && x.Item2 == y.Item1;
        }

        public int GetHashCode(Tuple<NetworkNode, NetworkNode> obj)
        {
            return obj.Item1.GetHashCode() ^ obj.Item2.GetHashCode();
        }
    }

    public void SwitchLayout(Layout layout)
    {
        if (layout == currentLayout)
            return;
        if (calculatingLayout || switchingLayout)
            return;

        StartCoroutine(SwitchLayoutCoroutine(layout, 2f));
    }

    private IEnumerator SwitchLayoutCoroutine(Layout layout, float time)
    {
        int newLayoutPositionIndex;
        int oldLayoutPositionIndex;
        if (layout == Layout.TWO_D)
        {
            newLayoutPositionIndex = 0;
            oldLayoutPositionIndex = 1;
        }
        else
        {
            newLayoutPositionIndex = 1;
            oldLayoutPositionIndex = 0;
        }

        if (!layoutsCalculated[newLayoutPositionIndex])
        {
            CalculateLayout(layout);
            yield break;
        }


        float t = 0f;
        float lerpBy = 0f;
        switchingLayout = true;
        while (t < 1f)
        {
            t += (Time.deltaTime / time);

            if (t > 1f)
                t = 1f;
            lerpBy = (Mathf.Sin(t * Mathf.PI - Mathf.PI / 2f) + 1f) / 2f;
            foreach (var node in nodes)
            {
                node.transform.localPosition = Vector3.Lerp(node.LayoutPositions[oldLayoutPositionIndex], node.LayoutPositions[newLayoutPositionIndex], lerpBy);
                node.RepositionEdges();
            }
            yield return null;
        }
        switchingLayout = false;
        currentLayout = layout;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            controllerInside = true;
            numColliders++;
            // i think this sometimes gets called before start
            // so let's make sure that the controllermodelswitcher is set
            if (controllerModelSwitcher != null)
                controllerModelSwitcher.SwitchToModel(ControllerModelSwitcher.Model.Menu);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Menu Controller Collider"))
        {
            numColliders--;
        }
        // We might collide with the network nodes' colliders. So OnTriggerExit is called a little too often,
        // so we must make sure we have exited all colliders.
        if (numColliders == 0)
        {
            controllerInside = false;
            controllerModelSwitcher.SwitchToDesiredModel();
        }
    }

    /// <summary>
    /// Hides the large sphere around the network if the network is enlarged. 
    /// The sphere should be hidden if the network is enlarged.
    /// </summary>
    internal void HideSphereIfEnlarged()
    {
        if (Enlarged)
        {
            GetComponent<Renderer>().enabled = false;
            GetComponent<Collider>().enabled = false;
        }
    }

    /// <summary>
    /// Called when the controller is inside the network and the trigger is pressed. Enlarges the network and seperates it from the skeleton and makes it movable by the user.
    /// </summary>
    public void EnlargeNetwork()
    {
        StartCoroutine(EnlargeNetworkCoroutine());
        CellexalEvents.NetworkEnlarged.Invoke();
    }

    private IEnumerator EnlargeNetworkCoroutine()
    {
        oldName = name;
        name = "Enlarged_" + name;
        Enlarged = true;
        GetComponent<Renderer>().enabled = false;
        GetComponent<Collider>().enabled = false;
        var rigidbody = gameObject.GetComponent<Rigidbody>();
        if (rigidbody == null)
        {
            rigidbody = gameObject.AddComponent<Rigidbody>();
        }
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
        rigidbody.angularDrag = float.PositiveInfinity;
        var interactableObject = gameObject.AddComponent<NetworkCenterInteract>();
        interactableObject.isGrabbable = true;
        interactableObject.isUsable = false;
        var grabAttach = gameObject.AddComponent<VRTK_FixedJointGrabAttach>();
        var scalescript = gameObject.AddComponent<VRTK_AxisScaleGrabAction>();
        scalescript.uniformScaling = true;
        interactableObject.grabAttachMechanicScript = grabAttach;
        interactableObject.secondaryGrabActionScript = scalescript;

        grabAttach.precisionGrab = true;
        grabAttach.breakForce = float.PositiveInfinity;

        // save the old variables
        oldParent = transform.parent;
        oldLocalPosition = transform.localPosition;
        oldScale = transform.localScale;
        oldRotation = transform.rotation;

        transform.parent = null;
        transform.position = pedestal.transform.position + new Vector3(0, 1, 0);
        transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
        transform.rotation = pedestal.transform.rotation;
        transform.Rotate(-90f, 0, 0);

        // instantiate a replacement in our place
        var replacement = Instantiate(replacementPrefab);
        replacement.transform.parent = oldParent;
        replacement.transform.localPosition = oldLocalPosition;
        replacement.transform.rotation = oldRotation;
        replacement.transform.localScale = oldScale;
        replacement.GetComponent<Renderer>().material.color = GetComponent<Renderer>().material.color;

        // make sure the replacement knows its place in the world
        var replacementScript = replacement.GetComponent<NetworkCenter>();
        replacementScript.isReplacement = true;
        replacementScript.replacing = this;
        replacementScript.Handler = Handler;

        // wait 1 frame before turning on the colliders, otherwise they all get triggered if
        // the controller is inside them
        yield return null;
        // turn on the colliders on the nodes so they can be highlighted
        foreach (BoxCollider b in GetComponentsInChildren<BoxCollider>())
        {
            b.enabled = true;
        }

        // tell our network handler that we have a replacement
        Handler.Replacements.Add(replacementScript);
    }

    /// <summary>
    /// If this network is enlarged, bring it back to the convex hull, if it is a replacement, destroy it and bring back the original 
    /// </summary>
    public void BringBackOriginal()
    {
        if (isReplacement)
        {
            replacing.BringBackOriginal();
            Handler.Replacements.Remove(this);
            // destroying without this also caused crashes
            Destroy(GetComponent<Collider>());
            Destroy(GetComponent<Renderer>());
            //rightController.gameObject.GetComponentInChildren<VRTK_InteractTouch>().ForceStopTouching();
            gameObject.SetActive(false);
            // calling Destroy without the time delay caused the program to crash pretty reliably
            Destroy(gameObject);
        }
        else
        {
            Enlarged = false;
            StartCoroutine(BringBackOriginalCoroutine());
        }
    }

    private IEnumerator BringBackOriginalCoroutine()
    {
        name = oldName;
        // the ForceStopInteracting waits until the end of the frame before it stops interacting
        // so we also have to wait one frame until proceeding
        gameObject.GetComponent<VRTK_InteractableObject>().ForceStopInteracting();
        yield return null;
        // now we can do things
        GetComponent<Renderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
        transform.parent = oldParent;
        transform.localPosition = oldLocalPosition;
        transform.rotation = oldRotation;
        transform.localScale = oldScale;
        // this network will now be part of the convex hull which already has a rigidbody and these scripts
        Destroy(gameObject.GetComponent<NetworkCenterInteract>());
        Destroy(gameObject.GetComponent<VRTK_AxisScaleGrabAction>());
        Destroy(gameObject.GetComponent<VRTK_InteractableObject>());
        Destroy(gameObject.GetComponent<Rigidbody>());

        if (transform.localScale.x > 5)
        {
            Debug.Log("DECREASE OBJ IN SKY");
            networkGenerator.objectsInSky--;
        }
        // we must wait one more frame here or VRTK_InteractTouch gets a bunch of null exceptions.
        // probably because it is still using these colliders
        yield return null;
        // Disable the network nodes' colliders
        foreach (Transform child in transform)
        {
            var node = child.GetComponent<NetworkNode>();
            if (node)
            {
                node.BringBack();
                node.GetComponent<BoxCollider>().enabled = false;
            }
        }
    }

    /// <summary>
    /// An arc is a line between two identical pairs of genes in two different networks
    /// </summary>
    private struct Arc
    {
        public LineRenderer renderer;
        public NetworkCenter center1, center2;
        // t1 and t2 are the genes' transforms in the first network
        // t3 and t4 are the genes' transforms in the second network
        public Transform t1, t2, t3, t4;
        // the gameobject that represents the arc
        public GameObject gameObject;

        public Arc(LineRenderer renderer, NetworkCenter center1, NetworkCenter center2, Transform t1, Transform t2, Transform t3, Transform t4, GameObject gameObject)
        {
            this.renderer = renderer;
            this.center1 = center1;
            this.center2 = center2;
            this.t1 = t1;
            this.t2 = t2;
            this.t3 = t3;
            this.t4 = t4;
            this.gameObject = gameObject;
        }
    }

    /// <summary>
    /// A combined arc is a line between two networks that represents the number of normal arcs between those networks.
    /// </summary>
    private struct CombinedArc
    {
        public LineRenderer renderer;
        public Transform center1, center2;
        public GameObject gameObject;
        public int nArcsCombined;

        public CombinedArc(LineRenderer renderer, Transform center1, Transform center2, int nArcsCombined, GameObject gameObject)
        {
            this.renderer = renderer;
            this.center1 = center1;
            this.center2 = center2;
            this.nArcsCombined = nArcsCombined;
            this.gameObject = gameObject;
        }
    }

    /// <summary>
    /// Adds an arc between two pairs of genes.
    /// </summary>
    /// <param name="n1"> The first gene in the first pair </param>
    /// <param name="n2"> The second gene in the first pair </param>
    /// <param name="n3"> The first gene in the second pair </param>
    /// <param name="n4"> The second gene in the second pair </param>
    internal void AddArc(NetworkNode n1, NetworkNode n2, NetworkNode n3, NetworkNode n4)
    {
        GameObject edge = Instantiate(edgePrefab);
        LineRenderer renderer = edge.GetComponent<LineRenderer>();
        edge.transform.parent = transform.parent;
        Vector3 midPoint1 = (n1.transform.position + n2.transform.position) / 2f;
        Vector3 midPoint2 = (n3.transform.position + n4.transform.position) / 2f;
        renderer.useWorldSpace = true;
        renderer.SetPositions(new Vector3[] { midPoint1, midPoint2 });

        Arc newArc = new Arc(renderer, n1.Center, n3.Center, n1.transform, n2.transform, n3.transform, n4.transform, edge);
        arcs.Add(newArc);
        n3.Center.arcs.Add(newArc);

        GameObject arcText = Instantiate(arcDescriptionPrefab);
        arcText.transform.parent = edge.transform;
        arcText.transform.position = (midPoint1 + midPoint2) / 2f;
        arcText.GetComponent<TextRotator>().SetTransforms(n1.transform, n2.transform, n3.transform, n4.transform);
        arcText.GetComponent<TextMesh>().text = n1.geneName.text + " <-> " + n2.geneName.text;
    }

    /// <summary>
    /// Shows or hides all normal arcs connected to this network.
    /// </summary>
    /// <param name="toggleToState"> The state to toggle to, true for visible false for invisible. </param>
    public void SetArcsVisible(bool toggleToState)
    {
        foreach (Arc arc in arcs)
        {
            arc.gameObject.SetActive(toggleToState);
        }
    }

    /// <summary>
    /// Shows or hides all combined arcs connected to this network.
    /// </summary>
    /// <param name="toggleToState"> The state to toggle to, true for visible false for invisible. </param>
    public void SetCombinedArcsVisible(bool toggleToState)
    {
        foreach (CombinedArc arc in combinedArcs)
        {
            arc.gameObject.SetActive(toggleToState);
        }
    }

    /// <summary>
    /// Creates combined arcs.
    /// A combined arc is a colored line that represents the number of normal arcs that go from this network to another.
    /// </summary>
    /// <returns> The maximum number of arcs that were combined to one. </returns>
    public int CreateCombinedArcs()
    {
        if (combinedArcs.Count > 0)
        {
            return 0;
        }

        Dictionary<NetworkCenter, int> nArcs = new Dictionary<NetworkCenter, int>();
        foreach (Arc arc in arcs)
        {
            if (arc.center1 != this)
            {
                if (nArcs.ContainsKey(arc.center1))
                    nArcs[arc.center1]++;
                else
                    nArcs[arc.center1] = 1;
            }
            else
            {
                if (nArcs.ContainsKey(arc.center2))
                    nArcs[arc.center2]++;
                else
                    nArcs[arc.center2] = 1;
            }
        }
        var max = 0;
        foreach (KeyValuePair<NetworkCenter, int> pair in nArcs)
        {
            if (pair.Key.combinedArcs.Count == 0)
            {
                if (pair.Value > max)
                    max = pair.Value;
                GameObject edge = Instantiate(edgePrefab);
                LineRenderer renderer = edge.GetComponent<LineRenderer>();
                edge.transform.parent = transform.parent;
                renderer.useWorldSpace = true;
                renderer.SetPositions(new Vector3[] { transform.position, pair.Key.transform.position });

                GameObject arcText = Instantiate(simpleArcDescriptionPrefab);
                arcText.transform.parent = edge.transform;
                arcText.transform.position = (transform.position + pair.Key.transform.position) / 2f;
                arcText.transform.localScale = arcText.transform.localScale * 2f;
                arcText.GetComponent<SimpleTextRotator>().SetTransforms(transform, pair.Key.transform);
                arcText.GetComponent<TextMesh>().text = "" + pair.Value;
                CombinedArc newArc = new CombinedArc(renderer, transform, pair.Key.transform, pair.Value, edge);
                combinedArcs.Add(newArc);
            }
        }
        return max;
    }

    /// <summary>
    /// Colors the combined arcs based on the their combined amount of arcs
    /// </summary>
    /// <param name="max"> The number of arcs that were combined at most. </param>
    internal void ColorCombinedArcs(int max)
    {
        foreach (CombinedArc arc in combinedArcs)
        {
            var colorIndex = (int)(Mathf.Floor(((float)(arc.nArcsCombined - 1) / max) * combinedArcsColors.Count));
            arc.renderer.startColor = combinedArcsColors[colorIndex];
            arc.renderer.endColor = combinedArcsColors[colorIndex];
        }
    }

    /// <summary>
    /// Saves this network as a text file
    /// </summary>
    public void SaveNetworkAsTextFile()
    {

        if (nodes.Count == 0)
            return;

        string directoryPath = CellexalUser.UserSpecificFolder;
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            CellexalLog.Log("Created directory " + directoryPath);
        }

        directoryPath += "\\Networks";
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
            CellexalLog.Log("Created directory " + directoryPath);
        }
        string filePath = directoryPath + "\\" + name + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".txt";
        var stream = File.Create(filePath);
        var streamWriter = new StreamWriter(stream);

        Dictionary<Tuple<NetworkNode, NetworkNode>, float> included = new Dictionary<Tuple<NetworkNode, NetworkNode>, float>(new TupleComparer());
        var nodeLocalPositionOffset = new Vector3(0.4f, 0.4f, 0f);

        // write the number of nodes
        streamWriter.WriteLine(nodes.Count);
        // write all the nodes and their positions
        foreach (var node in nodes)
        {
            var pos = node.transform.localPosition;
            pos += nodeLocalPositionOffset;
            streamWriter.WriteLine(node.Label + "\t" + pos.x + "\t" + pos.y);
        }

        // count and store the number of edges
        foreach (var node in nodes)
        {
            foreach (var edge in node.edges)
            {
                var pair = new Tuple<NetworkNode, NetworkNode>(edge.Item1, edge.Item2);
                if (included.ContainsKey(pair))
                {
                    continue;
                }
                included[pair] = edge.Item4;

            }
        }
        // write the number of edges
        streamWriter.WriteLine(included.Count);
        // write the edges and their pcor
        foreach (var edge in included)
        {
            streamWriter.WriteLine(edge.Key.Item1.Label + "\t" + edge.Key.Item2.Label + "\t" + edge.Value);
        }
        streamWriter.Close();
        stream.Close();

        CellexalLog.Log("Saved " + name + " as a text file at " + filePath);
    }

    /// <summary>
    /// Saves this network as an .png image
    /// </summary>
    public void SaveNetworkAsImage()
    {
        if (nodes.Count == 0)
            return;

        int bitmapWidth = 1024;
        int bitmapHeight = 1024;
        Bitmap bitmap = new Bitmap(bitmapWidth, bitmapHeight);
        var graphics = System.Drawing.Graphics.FromImage(bitmap);
        var lineBrushes = new Dictionary<Color, Pen>(new ColorComparer());
        var geneFont = new System.Drawing.Font(FontFamily.GenericMonospace, 12f, System.Drawing.FontStyle.Bold);
        Vector3 geneLocalPositionOffset = new Vector3(0.5f, 0.5f, 0f);

        foreach (var entry in networkGenerator.LineMaterials)
        {
            Color unitycolor = entry.color;
            lineBrushes[unitycolor] = new Pen(System.Drawing.Color.FromArgb((int)(unitycolor.r * 255), (int)(unitycolor.g * 255), (int)(unitycolor.b * 255)), 3f);
        }
        var thickerBlackBrush = new Pen(System.Drawing.Color.Black, 5f);
        var textFont = new System.Drawing.Font(FontFamily.GenericMonospace, 12f, System.Drawing.FontStyle.Bold);

        // draw a white background
        graphics.Clear(System.Drawing.Color.FromArgb(255, 255, 255));

        // draw the edges
        foreach (var node in nodes)
        {
            foreach (var edge in node.edges)
            {
                // The positions are generally between -0.4 and 0.4
                var pos1 = (edge.Item1.transform.localPosition + geneLocalPositionOffset);
                var pos2 = (edge.Item2.transform.localPosition + geneLocalPositionOffset);

                pos1.x *= bitmapWidth;
                pos2.x *= bitmapWidth;
                pos1.y *= bitmapHeight;
                pos2.y *= bitmapHeight;

                graphics.DrawLine(thickerBlackBrush, pos1.x, pos1.y, pos2.x, pos2.y);
                graphics.DrawLine(lineBrushes[edge.Item3.material.color], pos1.x, pos1.y, pos2.x, pos2.y);

            }
        }
        foreach (var node in nodes)
        {
            var bitmapPosition = node.transform.localPosition + geneLocalPositionOffset;
            bitmapPosition.x *= bitmapWidth;
            bitmapPosition.y *= bitmapHeight;
            graphics.FillEllipse(Brushes.Black, bitmapPosition.x - 5f, bitmapPosition.y - 5f, 10f, 10f);
        }
        // draw the gene names
        foreach (var node in nodes)
        {
            string nodeName = node.Label;
            var bitmapPosition = node.transform.localPosition + geneLocalPositionOffset;
            bitmapPosition.x *= bitmapWidth;
            bitmapPosition.y *= bitmapHeight;
            graphics.DrawString(nodeName, textFont, SystemBrushes.MenuText, bitmapPosition.x, bitmapPosition.y + 5f);
        }

        string networkImageDirectory = CellexalUser.UserSpecificFolder;
        if (!Directory.Exists(networkImageDirectory))
        {
            Directory.CreateDirectory(networkImageDirectory);
            CellexalLog.Log("Created directory " + networkImageDirectory);
        }

        networkImageDirectory += "\\Networks";
        if (!Directory.Exists(networkImageDirectory))
        {
            Directory.CreateDirectory(networkImageDirectory);
            CellexalLog.Log("Created directory " + networkImageDirectory);
        }

        string networkImageFilePath = networkImageDirectory + "\\" + name + "_" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + ".png";
        bitmap.Save(networkImageFilePath, ImageFormat.Png);

        CellexalLog.Log("Saved " + name + " as an image at " + networkImageFilePath);
    }

    /// <summary>
    /// This class looks stupid but is needed because unity represents colors with 3 floats that range from 0 to 1 
    /// and it likes to introduce precision errors when comparing colors that originally were the same.
    /// </summary>
    private class ColorComparer : IEqualityComparer<Color>
    {
        public bool Equals(Color x, Color y)
        {
            int xr = Mathf.RoundToInt(x.r * 255);
            int yr = Mathf.RoundToInt(y.r * 255);
            int xg = Mathf.RoundToInt(x.g * 255);
            int yg = Mathf.RoundToInt(y.g * 255);
            int xb = Mathf.RoundToInt(x.b * 255);
            int yb = Mathf.RoundToInt(y.b * 255);
            return xr == yr && xg == yg && xb == yb;
        }

        public int GetHashCode(Color obj)
        {
            int r = Mathf.RoundToInt(obj.r * 255);
            int g = Mathf.RoundToInt(obj.g * 255);
            int b = Mathf.RoundToInt(obj.b * 255);
            return r.GetHashCode() ^ g.GetHashCode() ^ b.GetHashCode();
        }
    }
}
