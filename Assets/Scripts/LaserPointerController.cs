using UnityEngine;
using VRTK;

public class LaserPointerController : MonoBehaviour {

// Use this for initialization
void Start () {
	GetComponent<VRTK_StraightPointerRenderer>().enabled = false;
}

}
