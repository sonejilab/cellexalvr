using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class H5ReaderAnnotatorScriptManager : MonoBehaviour
{

    public GameObject annotatorPrefab;
    public Dictionary<string,h5readerAnnotater> annotators = new Dictionary<string,h5readerAnnotater>();
    // Start is called before the first frame update
    void Start()
    {
        /*
        GameObject go = Instantiate(annotatorPrefab, transform);
        go.transform.localPosition = new Vector3(1.4f, 1.3f, 1.4f);
        go.transform.localEulerAngles = new Vector3(0, 0, 0);
        */
        addAnnotator("LCA_142K_umap_phate_loom");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void addAnnotator(string path)
    {
        if (annotators.ContainsKey(path))
        {
            annotators[path].gameObject.SetActive(true);
        }
        else
        {
            GameObject go = Instantiate(annotatorPrefab, transform);
            h5readerAnnotater script = go.GetComponent<h5readerAnnotater>();
            script.init(path);
            script.manager = this;
            annotators.Add(path, script);
            go.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            go.transform.localEulerAngles = new Vector3(0, 0, 0);
        }

    }

    public void removeAnnotator(string path)
    {

        annotators[path].gameObject.SetActive(false);
    }
}
