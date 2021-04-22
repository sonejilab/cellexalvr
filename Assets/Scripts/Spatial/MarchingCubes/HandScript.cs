using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;


//public class HandScript : MonoBehaviour
//{
//    public SteamVR_Action_Boolean pinchAction = null;
//    public SteamVR_Action_Boolean grabAction = null;
//    public SteamVR_Action_Boolean trackPadPressAction = null;

//    private SteamVR_Behaviour_Pose pose = null;

//    public ChunkManager cm;

//    private Joint joint = null;
//    public List<GameObject> objects = new List<GameObject>();
//    private GameObject currentGrabbable;

//    private bool isPincing;
//    private bool isPressingTrackPad;
    

//    // Start is called before the first frame update
//    void Awake()
//    {
//        pose = GetComponent<SteamVR_Behaviour_Pose>();
//        joint = GetComponent<Joint>(); 
//    }

//    // Update is called once per frame
//    void Update()
//    {
//        if (grabAction.GetLastStateDown(pose.inputSource))
//            grabActionPress();

//        if (grabAction.GetLastStateUp(pose.inputSource))
//            grabActionRelease();

//        if (pinchAction.GetLastStateDown(pose.inputSource))
//            pinch();

//        if (pinchAction.GetLastStateUp(pose.inputSource))
//            unpinch();

//        if (trackPadPressAction.GetLastStateDown(pose.inputSource))
//            trackPadPress();

//        if (trackPadPressAction.GetLastStateUp(pose.inputSource))
//            trackPadRelease();  


//        if (isPincing)
//        {
//            cm.addSphericalDensity(transform.position);
//        }

//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        objects.Add(other.gameObject);
//    }

//    private void OnTriggerExit(Collider other)
//    {
//        objects.Remove(other.gameObject);
//    }

//    private void grabActionPress()
//    {
//        currentGrabbable = getNearestWithTag("Grabbable");
//        if (currentGrabbable == null)
//            return;

//        joint.connectedBody = currentGrabbable.GetComponent<Rigidbody>();
//    }

//    private void grabActionRelease()
//    {
//        if (currentGrabbable == null)
//            return;

//        joint.connectedBody = null;
//        currentGrabbable = null;
//    }

//    private void trackPadPress()
//    {
//        isPressingTrackPad = true;
//    }

//    private void trackPadRelease()
//    {
//        isPressingTrackPad = false;
//    }

//    private void pinch()
//    {
//        isPincing = true;
//    }

//    private void unpinch()
//    {
//        isPincing = false;
//    }

//    private List<T> getWithScript<T>()
//    {
//        List<T> ret = new List<T>();

//        foreach(GameObject i in objects)
//        {
//            T boi = i.GetComponent<T>();
//            if(boi != null)
//                ret.Add(boi);
//        }
//        return ret;
//    }

//    private T getNearestWithScript<T>()
//    {
//        T nearest = default(T);
//        float minDist = float.MaxValue;
//        float dist = 0;
//        foreach (GameObject i in objects)
//        {
//            if (i.GetComponent<T>() == null)
//                continue;

//            dist = (i.transform.position - transform.position).sqrMagnitude;
//            if (dist < minDist)
//            {
//                minDist = dist;
//                nearest = i.GetComponent<T>();
//            }
//        }
//        return nearest;
//    }

//    private GameObject getNearestWithTag(string tag)
//    {
//        GameObject nearest = null;
//        float minDist = float.MaxValue;
//        float dist = 0;
//        foreach (GameObject i in objects)
//        {
//            if (!i.CompareTag(tag))
//                continue;
//            dist = (i.transform.position - transform.position).sqrMagnitude;
//            if (dist < minDist)
//            {
//                minDist = dist;
//                nearest = i;

//            }
//        }
//        return nearest;
//    }
//}
