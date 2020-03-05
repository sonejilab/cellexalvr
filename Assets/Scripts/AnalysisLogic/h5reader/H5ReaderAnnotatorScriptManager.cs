using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CellexalVR.AnalysisLogic.H5reader
{
    public class H5ReaderAnnotatorScriptManager : MonoBehaviour
    {

        public GameObject annotatorPrefab;
        public Dictionary<string, H5readerAnnotater> annotators = new Dictionary<string, H5readerAnnotater>();
        // Start is called before the first frame update
        void Start()
        {
            //string path = "LCA_142k_umap_phate_loom";

            //AddAnnotator("LCA_142k_umap_phate_loom");

            /*
            GameObject go = Instantiate(annotatorPrefab, transform);
            go.transform.localPosition = new Vector3(1.4f, 1.3f, 1.4f);
            go.transform.localEulerAngles = new Vector3(0, 0, 0);
            */
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void AddAnnotator(string path)
        {
            if (annotators.ContainsKey(path))
            {
                annotators[path].gameObject.SetActive(true);
            }
            else
            {
                GameObject go = Instantiate(annotatorPrefab, transform);
                H5readerAnnotater script = go.GetComponent<H5readerAnnotater>();
                script.Init(path);
                script.manager = this;
                annotators.Add(path, script);
                go.transform.localPosition = new Vector3(-0.5f, 1.5f, 0f);
                go.transform.localEulerAngles = new Vector3(0, 180f, 0);
            }

        }

        public void RemoveAnnotator(string path)
        {
            annotators[path].gameObject.SetActive(false);
        }
    }

}
