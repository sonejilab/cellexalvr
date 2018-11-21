using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPanel : MonoBehaviour {
    public Transform networkCenter;
    public Transform[] parents;
    // Use this for initialization
    void Start() {
        parents = GetComponentsInParent<Transform>();
        networkCenter = parents[3];
	}

    // Update is called once per frame
    void Update() {
        //transform.rotation = networkCenter.rotation * new Quaternion(-1, -1, -1, 0);
	}
}
