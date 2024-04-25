using CellexalVR.General;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace CellexalVR.AnalysisLogic.H5reader
{
    public class H5ReaderAnnotatorTextBoxScript : MonoBehaviour
    {
        public ReferenceManager referenceManager;
        // Open XR 
        //private SteamVR_Controller.Device device;
        private UnityEngine.XR.Interaction.Toolkit.ActionBasedController rightController;
        // Open XR 
        //private SteamVR_Controller.Device device;
        private UnityEngine.XR.InputDevice device;
        private bool controllerInside;
        public Dictionary<string, H5ReaderAnnotatorTextBoxScript> subkeys = new Dictionary<string, H5ReaderAnnotatorTextBoxScript>();
        public H5ReaderAnnotatorTextBoxScript parentScript;
        public GameObject textBoxPrefab;
        public RectTransform rect;
        public TextMeshProUGUI tmp;
        public BoxCollider boxCollider;

        public RectTransform expandButtonRect;
        public BoxCollider expandButtonBoxCollider;
        public GameObject KeyObject;

        public H5readerAnnotater annotater;


        public string annotationName;
        public bool isTop;
        public bool isBottom = false;
        private Color hoverColor = Color.white;
        public Color color = Color.black;
        private Color highlightColor = Color.yellow;
        private bool highLightOn = false;
        public string type = "none";
        public bool isSelected = false;
        private float timer = 0f;

        private void Start()
        {
            if (!referenceManager)
            {
                referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
            }
            rightController = referenceManager.rightController;

            CellexalEvents.RightTriggerClick.AddListener(OnTriggerPressed);
        }

        public void Insert(string name, H5readerAnnotater annotaterScript)
        {
            annotater = annotaterScript;
            if (name.Contains("/"))
            {
                string parentKey = name.Substring(0, name.IndexOf("/"));
                string newName = name.Substring(name.IndexOf("/") + 1);
                if (!subkeys.ContainsKey(parentKey))
                {
                    GameObject go = Instantiate(textBoxPrefab);
                    H5ReaderAnnotatorTextBoxScript script = go.GetComponent<H5ReaderAnnotatorTextBoxScript>();
                    script.isTop = false;
                    script.annotationName = parentKey;
                    subkeys.Add(parentKey, script);
                    script.parentScript = this;
                }
                subkeys[parentKey].Insert(newName, annotaterScript);
            }
            else
            {
                GameObject go = Instantiate(textBoxPrefab);
                H5ReaderAnnotatorTextBoxScript script = go.GetComponent<H5ReaderAnnotatorTextBoxScript>();
                script.annotationName = name;
                subkeys.Add(name, script);
                script.isTop = false;
                script.isBottom = true;
                script.parentScript = this;
                if (!isTop)
                    go.SetActive(false);
            }
        }

        public char GetDataType()
        {
            char dtype = annotationName[annotationName.Length - 1];
            return dtype;
        }

        public string GetPath()
        {
            string path = annotationName.Substring(0, annotationName.LastIndexOf(":"));
            H5ReaderAnnotatorTextBoxScript p = parentScript;
            while (p.isTop == false)
            {
                path = p.annotationName + "/" + path;
                p = p.parentScript;
            }
            return path;
        }

        public ArrayList GetTypeInChildren(string type)
        {
            ArrayList list = new ArrayList();
            foreach (H5ReaderAnnotatorTextBoxScript k in subkeys.Values)
            {
                list.AddRange(k.GetTypeInChildren(type));
                if (k.type == type)
                {
                    list.Add(k);
                }
            }
            return list;
        }

        public void FillContent(RectTransform content, int depth = 0)
        {
            gameObject.name = annotationName;
            transform.SetParent(content);

            rect.localPosition = Vector3.zero;
            rect.localEulerAngles = Vector3.zero;
            rect.localScale = new Vector3(1, 1, 1);

            rect.anchorMin = new Vector2(0, 0);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);

            tmp.fontSize = 8;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            string text = "";
            for (int i = 0; i < depth; i++)
            {
                text += "--";
            }
            text += "> ";
            tmp.text = text + annotationName;

            foreach (H5ReaderAnnotatorTextBoxScript k in subkeys.Values)
            {
                k.FillContent(rect, depth + 1);
            }
        }

        public float UpdatePosition(float offset = 0f)
        {
            rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, offset, 0);
            rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 5f, 0);
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            boxCollider.center = new Vector3(rect.rect.width / 2, -rect.rect.height / 2, 0);
            boxCollider.size = new Vector3(rect.rect.width, rect.rect.height, 1);

            expandButtonRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 10f);
            expandButtonRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, -10, 10f);
            expandButtonBoxCollider.size = new Vector3(expandButtonRect.rect.size.x, expandButtonRect.rect.size.y, 5);



            float temp = 0f;
            foreach (H5ReaderAnnotatorTextBoxScript k in subkeys.Values)
            {
                if (k.gameObject.activeSelf)
                {
                    temp += 10f;
                    temp += k.UpdatePosition(temp);
                }
            }
            return temp;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
            {
                controllerInside = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
            {
                controllerInside = false;
            }
        }

        private void Update()
        {
            // Open XR
            //device = SteamVR_Controller.Input((int)rightController.index);

        }

        private void OnTriggerPressed()
        {
            if (controllerInside)
            {

            }
            if (isSelected)
            {
                timer += UnityEngine.Time.deltaTime;
                if (timer > 0.5f)
                {
                    timer = 0f;
                    if (highLightOn)
                    {
                        tmp.color = color;
                        highLightOn = false;
                    }
                    else
                    {
                        tmp.color = highlightColor;
                        highLightOn = true;
                    }
                }
            }

        }
    }
}


