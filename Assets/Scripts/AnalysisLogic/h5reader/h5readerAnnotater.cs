using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Linq;
using CellexalVR.AnalysisObjects;
using SQLiter;
using CellexalVR.General;
using TMPro;

public class h5readerAnnotater : MonoBehaviour
{
    public RectTransform display;
    public GameObject textBoxPrefab;
    Process p;
    StreamReader reader;
    GameObject firstTextBox;
    H5ReaderAnnotatorTextBoxScript keys;

    void Start()
    {
        p = new Process();
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardInput = true;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;
        startInfo.WindowStyle = ProcessWindowStyle.Minimized;

        startInfo.FileName = "py.exe";

        string file_name = "LCA_142K_umap_phate.loom";
        startInfo.Arguments = "crawl.py " + "Data/" + file_name;
        p.StartInfo = startInfo;
        p.Start();
        reader = p.StandardOutput;

        //Read all keys from the loom file
        GameObject go = Instantiate(textBoxPrefab);
        keys = go.GetComponent<H5ReaderAnnotatorTextBoxScript>();
        go.name = file_name;
        keys.name = file_name;
        keys.isTop = true;
        string standard_output;
        while ((standard_output = reader.ReadLine()) != null)
        {
            if (standard_output.Contains("xx"))
                break;
            if (!standard_output.Contains("("))
                continue;
            keys.insert(standard_output);
        }
        keys.fillContent(display);
        float contentSize = keys.updatePosition(10f);
        display.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, contentSize);
    }

    public static void testPrint()
    {
        print("test");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            keys.updatePosition(10f);
        }
    }

    //class KeyHolder : MonoBehaviour
    //{
    //    public Dictionary<string,KeyHolder> subkeys = new Dictionary<string, KeyHolder>();
    //    public KeyHolder parent;
    //    public string name;
    //    public GameObject go;
    //    public RectTransform rect;
    //    public TextMeshProUGUI tmp;
    //    public BoxCollider boxCollider;

    //    public KeyHolder(string name)
    //    {
    //        this.name = name;
    //    }

    //    public void insert(string name)
    //    {
    //        if (name.Contains("/"))
    //        {
    //            string parentKey = name.Substring(0, name.IndexOf("/"));
    //            string newName = name.Substring(name.IndexOf("/") + 1);
    //            if(!subkeys.ContainsKey(parentKey))
    //                subkeys.Add(parentKey, new KeyHolder(parentKey));
    //            subkeys[parentKey].insert(newName);
    //        }
    //        else
    //        {
    //            subkeys.Add(name, new KeyHolder(name));
    //        }
    //    }

    //    public string outputPrint(string push = "")
    //    {
    //        string ret = push + name + "\n";
    //        foreach (KeyHolder k in subkeys.Values)
    //            ret+=k.outputPrint(push + "|--");
    //        return ret;
    //    }

    //    public void fillContent(RectTransform content, int depth = 0)
    //    {
    //        go = new GameObject(name);
    //        go.transform.SetParent(content);
    //        rect = go.AddComponent<RectTransform>();
    //        go.AddComponent<CanvasRenderer>();
    //        tmp = go.AddComponent<TextMeshProUGUI>();
    //        boxCollider = go.AddComponent<BoxCollider>();
    //        boxCollider.isTrigger = true;

    //        rect.localScale = new Vector3(1, 1, 1);
    //        rect.anchorMin = new Vector2(0, 1);
    //        rect.anchorMax = new Vector2(1, 0);
    //        rect.pivot = new Vector2(0.5f, 1);
    //        rect.localPosition = Vector3.zero;
    //        rect.localEulerAngles = Vector3.zero;

    //        tmp.fontSize = 8;
    //        tmp.alignment = TextAlignmentOptions.MidlineLeft;
    //        string text = "";
    //        for (int i = 0; i < depth; i++)
    //        {
    //            text += "--";
    //        }
    //        text += "> ";
    //        tmp.text = text + name;

    //        foreach (KeyHolder k in subkeys.Values)
    //        {
    //            k.fillContent(rect, depth + 1);
    //        }
    //    }

    //    public float updatePosition(float offset = 0f)
    //    {
    //        rect.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, offset, 10f);
    //        rect.sizeDelta = new Vector2(0,rect.sizeDelta.y);
    //        boxCollider.center = new Vector3(0, -5,0);
    //        boxCollider.size = new Vector3(160, 10,1);
    //        float temp = 0f;
    //        foreach (KeyHolder k in subkeys.Values)
    //        {
    //            if (k.go.activeSelf)
    //            {
    //                temp += 10f;
    //                temp += k.updatePosition(temp);
    //            }
    //        }
    //        return temp;
    //    }
    //}
}
