using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineScript : MonoBehaviour
{
    public Transform AnchorA;
    public Transform AnchorB;
    public LineRenderer line;
    public ProjectionObjectScript projectionObjectScript;
    public string type;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        line.SetPosition(0, AnchorA.position);
        line.SetPosition(1, AnchorB.position);
    }
}
