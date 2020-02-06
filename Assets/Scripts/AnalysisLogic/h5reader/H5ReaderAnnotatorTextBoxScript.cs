using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using CellexalVR.General;
using UnityEngine.UI;
using System.IO;

public class H5ReaderAnnotatorTextBoxScript : MonoBehaviour
{
    public ReferenceManager referenceManager;
    private SteamVR_TrackedObject rightController;
    private SteamVR_Controller.Device device;
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

    public h5readerAnnotater annotater;


    public string name;
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
    }

    public void insert(string name, h5readerAnnotater annotaterScript)
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
                script.name = parentKey;
                subkeys.Add(parentKey, script);
                script.parentScript = this;
            }
            subkeys[parentKey].insert(newName, annotaterScript);
        }
        else
        {
            GameObject go = Instantiate(textBoxPrefab);
            H5ReaderAnnotatorTextBoxScript script = go.GetComponent<H5ReaderAnnotatorTextBoxScript>();
            script.name = name;
            subkeys.Add(name, script);
            script.isTop = false;
            script.isBottom = true;
            script.parentScript = this;
        }
    }

    public string getPath()
    {
        string path = name.Substring(0, name.LastIndexOf(":"));
        H5ReaderAnnotatorTextBoxScript p = parentScript;
        while(p.isTop == false)
        {
            path = p.name + Path.DirectorySeparatorChar + path;
            p = p.parentScript;
        }
        return path;
    }

    public ArrayList getTypeInChildren(string type)
    {
        ArrayList list = new ArrayList();
        foreach (H5ReaderAnnotatorTextBoxScript k in subkeys.Values)
        {
            list.AddRange(k.getTypeInChildren(type));
            if (k.type == type)
            {
                list.Add(k);
            }
        }
        return list;
    }

    public void fillContent(RectTransform content, int depth = 0)
    {
        gameObject.name = name;
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
        tmp.text = text + name;

        foreach (H5ReaderAnnotatorTextBoxScript k in subkeys.Values)
        {
            k.fillContent(rect, depth + 1);
        }
    }

    public float updatePosition(float offset = 0f)
    {
        rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, offset, 0);
        rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Left, 5f, 0);
        LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        boxCollider.center = new Vector3(rect.rect.width/2, -rect.rect.height/2, 0);
        boxCollider.size = new Vector3(rect.rect.width, rect.rect.height, 1);

        expandButtonRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, 10f);
        expandButtonRect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Right, -10, 10f);
        expandButtonBoxCollider.size = expandButtonRect.rect.size;

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
            tmp.color = hoverColor;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name.Equals("ControllerCollider(Clone)"))
        {
            controllerInside = false;
            tmp.color = color;
        }
    }

    private void Update()
    {
        device = SteamVR_Controller.Input((int)rightController.index);
        if (controllerInside && device.GetPressDown(SteamVR_Controller.ButtonMask.Trigger))
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
                }else{
                    tmp.color = highlightColor;
                    highLightOn = true;
                }
            }
        }

    }
}



