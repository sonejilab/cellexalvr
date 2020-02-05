using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class H5ReaderAnnotatorScriptManager : MonoBehaviour
{

    public GameObject annotatorPrefab;
    // Start is called before the first frame update
    void Start()
    {
        GameObject go = Instantiate(annotatorPrefab, transform);
        go.transform.localPosition = new Vector3(1.4f, 1.3f, 1.4f);
        go.transform.localEulerAngles = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
