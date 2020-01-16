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
        go.transform.localPosition = new Vector3(0.8f, 1.0f, -1.27f);
        go.transform.localEulerAngles = new Vector3(0, 0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
