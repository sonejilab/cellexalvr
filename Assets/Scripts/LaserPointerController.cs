using UnityEngine;
using VRTK;

public class LaserPointerController : MonoBehaviour {
    //public VRTK_StraightPointerRenderer renderer;


    // Use this for initialization
    void Start () {
       GetComponent<VRTK_StraightPointerRenderer>().enabled = false;
	}

}
