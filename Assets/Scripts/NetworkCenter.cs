using System;
using System.Collections.Generic;
using UnityEngine;
using VRTK;
using VRTK.GrabAttachMechanics;
using VRTK.SecondaryControllerGrabActions;

/// <summary>
/// This class represents the center of a network. It handles the enlarging when it is pressed.
/// </summary>
public class NetworkCenter : MonoBehaviour
{
    public GameObject replacementPrefab;
    public GameObject edgePrefab;
    public GameObject arcDescriptionPrefab;
    public GameObject simpleArcDescriptionPrefab;
    public List<Color> combinedArcsColors;
    public NetworkHandler Handler { get; set; }
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


    private bool isReplacement = false;

    [HideInInspector]
    public NetworkCenter replacing;
    private List<Arc> arcs = new List<Arc>();
    private List<CombinedArc> combinedArcs = new List<CombinedArc>();
    private SteamVR_TrackedObject rightController;
    private NetworkGenerator networkGenerator;

    void Start()
    {
        pedestal = GameObject.Find("Pedestal");
        rightController = GameObject.Find("Controller (right)").GetComponent<SteamVR_TrackedObject>();
        networkGenerator = GameObject.Find("NetworkGenerator").GetComponent<NetworkGenerator>();

    }

    void FixedUpdate()
    {
        // moving kinematic rigidbodies
        if (enlarge)
        {
            enlarge = false;
            if (!isReplacement && this.name != "Enlarged Network")
            {
                EnlargeNetwork();
            }
            else if (isReplacement && this.name == "EmptyNetworkPrefab 1(Clone)")
            {

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
            enlarge = true;
        }

        foreach (Arc a in arcs)
        {
            Vector3 midPoint1 = (a.t1.transform.position + a.t2.transform.position) / 2f;
            Vector3 midPoint2 = (a.t3.transform.position + a.t4.transform.position) / 2f;
            a.renderer.SetPositions(new Vector3[] { midPoint1, midPoint2 });
        }

        foreach (CombinedArc a in combinedArcs)
        {
            if (a.center1 != this)
                a.renderer.SetPositions(new Vector3[] { transform.position, a.center2.transform.position });
            else
                a.renderer.SetPositions(new Vector3[] { transform.position, a.center1.transform.position });
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Smaller Controller Collider"))
        {

            controllerInside = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Smaller Controller Collider"))
        {
            controllerInside = false;
        }
    }

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

        this.name = "Enlarged Network";
        // add a rigidbody and the necessary scripts
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
        var interactableObject = gameObject.AddComponent<VRTK_InteractableObject>();
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
    private void BringBackOriginal()
    {

        this.name = "Original Network";
        if (isReplacement)
        {
            replacing.BringBackOriginal();
            Handler.Replacements.Remove(this);
            // destroying without this also caused crashes
            Destroy(GetComponent<Collider>());
            Destroy(GetComponent<Renderer>());
            rightController.gameObject.GetComponentInChildren<VRTK_InteractTouch>().ForceStopTouching();
            gameObject.SetActive(false);
            // calling Destroy without the time delay caused the program to crash pretty reliably
            Destroy(gameObject, 0.1f);
        }
        else
        {
            Enlarged = false;
            // this network will now be part of the convex hull which already has a rigidbody and these scripts
            Destroy(gameObject.GetComponent<VRTK_FixedJointGrabAttach>());
            Destroy(gameObject.GetComponent<VRTK_AxisScaleGrabAction>());
            Destroy(gameObject.GetComponent<VRTK_InteractableObject>());
            Destroy(gameObject.GetComponent<Rigidbody>());
            Debug.Log(transform.localScale);
            if (transform.localScale.x > 5)
            {
                Debug.Log("DECREASE OBJ IN SKY");
                networkGenerator.objectsInSky--;
            }
            GetComponent<Renderer>().enabled = true;
            GetComponent<Collider>().enabled = true;
            transform.parent = oldParent;
            transform.localPosition = oldLocalPosition;
            transform.rotation = oldRotation;
            transform.localScale = oldScale;
            // Disable the network nodes' colliders
            foreach (BoxCollider b in GetComponentsInChildren<BoxCollider>())
            {
                b.enabled = false;
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
        n3.Center.AddArcToList(newArc);

        GameObject arcText = Instantiate(arcDescriptionPrefab);
        arcText.transform.parent = edge.transform;
        arcText.transform.position = (midPoint1 + midPoint2) / 2f;
        arcText.GetComponent<TextRotator>().SetTransforms(n1.transform, n2.transform, n3.transform, n4.transform);
        arcText.GetComponent<TextMesh>().text = n1.geneName.text + " <-> " + n2.geneName.text;
    }

    private void AddArcToList(Arc arc)
    {
        arcs.Add(arc);
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
}
