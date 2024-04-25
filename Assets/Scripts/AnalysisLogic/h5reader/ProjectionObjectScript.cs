using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CellexalVR.AnalysisLogic.H5reader
{
    public class ProjectionObjectScript : MonoBehaviour
    {
        public enum projectionType
        {
            p3D,
            p2D_sep,
            p3D_sep,
            p2D
        }

        public projectionType type;
        public string prohectionName = "unnamed-projection";
        private Dictionary<string, string> paths;
        private Dictionary<string, char> dataTypes;

        public GameObject AnchorPrefab;
        public H5readerAnnotater h5readerAnnotater;
        public TextMeshProUGUI seperatedText;
        private List<GameObject> instantiatedGameObjects;

        private Dictionary<projectionType, string[]> menu_setup;
        private TextMeshProUGUI nameTextMesh;

        void Start()
        {
            paths = new Dictionary<string, string>();
            dataTypes = new Dictionary<string, char>();
            nameTextMesh = transform.GetChild(0).GetComponentInChildren<TextMeshProUGUI>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void AddToPaths(string key, string value, char dtype)
        {
            if (!paths.ContainsKey(key))
            {
                paths.Add(key, value);
                dataTypes.Add(key, dtype);
            }
            else
            {
                paths[key] = value;
                dataTypes[key] = dtype;
            }
            h5readerAnnotater.AddToConfig(key + "_" + prohectionName, value, dtype);
        }

        public void RemoveFromPaths(string key)
        {
            if (paths.ContainsKey(key))
            {
                paths.Remove(key);
            }
            if (dataTypes.ContainsKey(key))
                dataTypes.Remove(key);

            h5readerAnnotater.RemoveFromConfig(key + "_" + prohectionName);

        }

        public void SwitchToSeparate()
        {


            switch (type)
            {
                case projectionType.p3D:
                    type = projectionType.p3D_sep;
                    break;
                case projectionType.p2D_sep:
                    type = projectionType.p2D;
                    break;
                case projectionType.p3D_sep:
                    type = projectionType.p3D;
                    break;
                case projectionType.p2D:
                    type = projectionType.p2D_sep;
                    break;
            }

            if (seperatedText.text == "sep")
            {
                seperatedText.text = "unsep";
            }
            else if (seperatedText.text == "unsep")
            {
                seperatedText.text = "sep";
            }

            foreach (string key in paths.Keys)
                h5readerAnnotater.RemoveFromConfig(key + "_" + prohectionName);

            Init(type);
        }


        public void ChangeName(string name)
        {
            print(name);
            this.prohectionName = name;
            nameTextMesh.text = name;
        }

        public void OnDestroy()
        {
            foreach (string key in paths.Keys)
                h5readerAnnotater.RemoveFromConfig(key + "_" + prohectionName);

            h5readerAnnotater.projectionObjectScripts.Remove(this);
            Destroy(this.gameObject);
            RectTransform rect;
            int counter = 0;
            foreach (ProjectionObjectScript p in h5readerAnnotater.projectionObjectScripts)
            {
                rect = p.GetComponent<RectTransform>();
                rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, rect.rect.width * (1.1f) * counter++, rect.rect.width);
            }
        }

        public void Init(projectionType type)
        {
            this.type = type;
            paths = new Dictionary<string, string>();
            if (instantiatedGameObjects != null)
            {
                foreach (GameObject g in instantiatedGameObjects)
                    Destroy(g);
            }
            instantiatedGameObjects = new List<GameObject>();

            if (type == projectionType.p2D_sep || type == projectionType.p3D_sep)
                seperatedText.text = "unsep";
            else
                seperatedText.text = "sep";

            menu_setup = new Dictionary<projectionType, string[]>
            {
                 { projectionType.p3D, new string[] { "X", "vel" } },
                 { projectionType.p2D_sep, new string[] { "X", "Y", "velX", "velY" } },
                 { projectionType.p3D_sep, new string[] { "X", "Y", "Z", "velX", "velY", "velZ" } },
                 { projectionType.p2D, new string[] { "X", "velX"} }
            };

            int offset = 0;
            GameObject go;

            foreach (string anchor in menu_setup[type])
            {
                go = Instantiate(AnchorPrefab, gameObject.transform, false);
                go.transform.localPosition += Vector3.up * -10 * offset;
                go.GetComponentInChildren<LineScript>().type = anchor;
                offset++;
                go.GetComponent<TextMeshProUGUI>().text = anchor;
                instantiatedGameObjects.Add(go);
            }
        }
    }
}
