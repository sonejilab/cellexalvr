//using VRTK;
//using UnityEngine;
//using System.Collections.Generic;

///// <summary>
///// This class is responsible for elarging networks and returning them to normal size
///// </summary>
//public class EnlargeNetworkInteract : MonoBehaviour
//{

//    public GameObject replacementPrefab;
//    public NetworkCenter script;

//    private Color normalColor;
//    private Color highlightColor;

//    private GameObject pedestal;
//    private SteamVR_Controller.Device device;
//    private bool controllerInside = false;
//    private Vector3 oldLocalPosition;
//    private Vector3 oldScale;
//    private Quaternion oldRotation;
//    private Transform oldParent;
//    public bool Enlarged { get; private set; }
//    private bool enlarge = false;
//    private bool isReplacement = false;
//    private EnlargeNetworkInteract replacing;
//    private List<GameObject> arcs = new List<GameObject>();
//    private SteamVR_ControllerManager cm;
//    private SteamVR_TrackedObject rightController;

//    public void EnlargeNetwork()
//    {
//        // add a rigidbody and the necessary scripts
//        Enlarged = true;
//        GetComponent<Renderer>().enabled = false;
//        GetComponent<Collider>().enabled = false;
//        var rigidbody = gameObject.AddComponent<Rigidbody>();
//        rigidbody.useGravity = false;
//        rigidbody.isKinematic = true;
//        rigidbody.angularDrag = float.PositiveInfinity;
//        var interactableObject = gameObject.GetComponent<VRTK_InteractableObject>();
//        interactableObject.isGrabbable = true;
//        //var grabAttach = gameObject.AddComponent<VRTK_FixedJointGrabAttach>();
//        //var scalescript = gameObject.AddComponent<VRTK_AxisScaleGrabAction>();
//        //scalescript.uniformScaling = true;
//        //grabAttachMechanicScript = grabAttach;
//        //secondaryGrabActionScript = scalescript;

//        //grabAttach.precisionGrab = true;
//        //grabAttach.breakForce = float.PositiveInfinity;

//        // save the old variables
//        oldParent = transform.parent;
//        oldLocalPosition = transform.localPosition;
//        oldScale = transform.localScale;
//        oldRotation = transform.rotation;

//        transform.parent = null;
//        transform.position = pedestal.transform.position + new Vector3(0, 1, 0);
//        transform.localScale = new Vector3(.7f, .7f, .7f);
//        transform.rotation = pedestal.transform.rotation;
//        transform.Rotate(-90f, 0, 0);

//        // instantiate a replacement in our place
//        var replacement = Instantiate(replacementPrefab);
//        replacement.transform.parent = oldParent;
//        replacement.transform.localPosition = oldLocalPosition;
//        replacement.transform.rotation = oldRotation;
//        replacement.transform.localScale = oldScale;
//        replacement.GetComponent<Renderer>().material.color = GetComponent<Renderer>().material.color;

//        // make sure the replacement knows its place in the world
//        var replacementScript = replacement.GetComponent<EnlargeNetworkInteract>();
//        replacementScript.isReplacement = true;
//        replacementScript.replacing = this;
//    }

//    private void BringBackOriginal()
//    {
//        if (isReplacement)
//        {
//            replacing.BringBackOriginal();
//            Destroy(gameObject);
//        }
//        else
//        {
//            Enlarged = false;

//            // this network will now be part of the convex hull which already has a rigidbody and these scripts
//            GetComponent<VRTK_InteractableObject>().isGrabbable = false;
//            //Destroy(gameObject.GetComponent<VRTK_InteractableObject>());
//            Destroy(gameObject.GetComponent<Rigidbody>());
//            GetComponent<Renderer>().enabled = true;
//            GetComponent<Collider>().enabled = true;
//            transform.parent = oldParent;
//            transform.localPosition = oldLocalPosition;
//            transform.rotation = oldRotation;
//            transform.localScale = oldScale;
//            ////var rigidbody = gameObject.GetComponentInParent<Rigidbody>();
//            //var rigidbody2 = rigidbody.gameObject.GetComponentInParent<Rigidbody>();
//            //rigidbody2.transform.Translate(.1f, 0, 0);
//            //print(oldParent.name);
//            //oldParent.Translate(0, 0, 0);
//            //rigidbody.transform.Translate(-1, 0, 0);
//            //print(GetComponent<Collider>().bounds.center);
//        }
//    }
//}
