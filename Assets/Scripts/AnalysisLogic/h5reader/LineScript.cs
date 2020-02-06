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
    public ArrayList subLines = new ArrayList();
    public GameObject linePrefab;
    private h5readerAnnotater h5ReaderAnnotater;
    // Start is called before the first frame update
    void Start()
    {
        h5ReaderAnnotater = GetComponentInParent<h5readerAnnotater>();
        line.positionCount = 10;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 dir = -h5ReaderAnnotater.transform.forward;
        Vector3 start = AnchorA.position;
        Vector3 end = AnchorB.position;
        if(start == end)
        {
            for (int i = 0; i < 10; i++)
            {
                line.SetPosition(i, start);
            }
        }
        else
        {
            for (int i = 0; i < 10; i++)
            {
                Vector3 pos = Vector3.Lerp(start, end, i / 9f) + dir * 0.1f * Mathf.Sin(Mathf.PI * (i / 9f));
                line.SetPosition(i, pos);
            }
        }
    }
}
