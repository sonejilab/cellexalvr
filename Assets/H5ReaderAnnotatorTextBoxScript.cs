using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CellexalVR.General;

public class H5ReaderAnnotatorTextBoxScript : MonoBehaviour
{
    public ReferenceManager referenceManager;
    private SteamVR_TrackedObject rightController;
    private SteamVR_Controller.Device device;
    private bool controllerInside;
    public Dictionary<string, H5ReaderAnnotatorTextBoxScript> subkeys = new Dictionary<string, H5ReaderAnnotatorTextBoxScript>();
    public GameObject textBoxPrefab;
    public RectTransform rect;
    public TextMeshProUGUI tmp;
    public BoxCollider boxCollider;
    public string name;
    public bool isTop = false;

    private void Start()
    {
        if (!referenceManager)
        {
            referenceManager = GameObject.Find("InputReader").GetComponent<ReferenceManager>();
        }
        rightController = referenceManager.rightController;
    }

    public void insert(string name)
    {
        if (name.Contains("/"))
        {
            string parentKey = name.Substring(0, name.IndexOf("/"));
            string newName = name.Substring(name.IndexOf("/") + 1);
            if (!subkeys.ContainsKey(parentKey))
            {
                GameObject go = Instantiate(textBoxPrefab);
                H5ReaderAnnotatorTextBoxScript script = go.GetComponent<H5ReaderAnnotatorTextBoxScript>();
                script.name = parentKey;
                subkeys.Add(parentKey, script);
            }
            subkeys[parentKey].insert(newName);
        }
        else
        {
            GameObject go = Instantiate(textBoxPrefab);
            H5ReaderAnnotatorTextBoxScript script = go.GetComponent<H5ReaderAnnotatorTextBoxScript>();
            script.name = name;
            subkeys.Add(name, script);
        }
    }

    public void fillContent(RectTransform content, int depth = 0)
    {
        gameObject.name = name;
        transform.SetParent(content);

        rect.localScale = new Vector3(1, 1, 1);
        rect.anchorMin = new Vector2(0, 1);
        rect.anchorMax = new Vector2(1, 0);
        rect.pivot = new Vector2(0.5f, 1);
        rect.localPosition = Vector3.zero;
        rect.localEulerAngles = Vector3.zero;

        tmp.fontSize = 8;
        tmp.alignment = TextAlignmentOptions.MidlineLeft;
        string text = "";
        for (int i = 0; i < depth; i++)
        {
            text += "--";
        }
        text += "> ";
        tmp.text = text + name;

        foreach (H5ReaderAnnotatorTextBoxScript k in subkeys.Values)
        {
            k.fillContent(rect, depth + 1);
        }
    }

    public float updatePosition(float offset = 0f)
    {
        rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, offset, 10f);
        rect.sizeDelta = new Vector2(0, rect.sizeDelta.y);
        boxCollider.center = new Vector3(0, -5, 0);
        boxCollider.size = new Vector3(160, 10, 1);
        float temp = 0f;
        foreach (H5ReaderAnnotatorTextBoxScript k in subkeys.Values)
        {
            if (k.gameObject.activeSelf)
            {
                temp += 10f;
                temp += k.updatePosition(temp);
            }
        }
        return temp;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
        {
            controllerInside = true;
            tmp.color = Color.red;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
        {
            controllerInside = false;
            tmp.color = Color.white;
        }
    }

    private void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
        {
            foreach (H5ReaderAnnotatorTextBoxScript key in subkeys.Values)
            {
                key.gameObject.SetActive(!key.gameObject.activeSelf);
            }
            //H5ReaderAnnotatorTextBoxScript parent = this;
            //while (!parent.isTop)
            //    parent = GetComponentInParent<H5ReaderAnnotatorTextBoxScript>();
            //parent.updatePosition(10f);
        }
    }
}



