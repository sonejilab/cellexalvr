using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace CellexalVR.AnalysisLogic.H5reader
{
    public class H5ReaderAnnotatorScriptManager : MonoBehaviour
    {
        public GameObject annotatorPrefab;
        public Dictionary<string, H5readerAnnotater> annotators = new Dictionary<string, H5readerAnnotater>();
        private ReferenceManager referenceManager;
        // Start is called before the first frame update
        private void Start()
        {
            if (!referenceManager)
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
        }

        public void AddAnnotator(string path)
        {
            if (annotators.ContainsKey(path))
            {
                annotators[path].gameObject.SetActive(true);
            }
            else
            {
                GameObject go = Instantiate(annotatorPrefab, this.transform);
                go.transform.parent = transform;
                go.transform.localPosition = Vector3.zero;
                H5readerAnnotater script = go.GetComponent<H5readerAnnotater>();
                script.Init(path);
                annotators.Add(path, script);
            }
        }

        public void RemoveAnnotator(string path)
        {
            annotators[path].gameObject.SetActive(false);
        }
    }

}
