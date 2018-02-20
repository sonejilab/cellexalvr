using UnityEngine;
using VRTK;
/// <summary>
/// Turns off a laser pointer when the program starts.
/// </summary>
public class LaserPointerController : MonoBehaviour
{

    // Use this for initialization
    void Start()
    {
        GetComponent<VRTK_StraightPointerRenderer>().enabled = false;
    }
}
